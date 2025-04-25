// YamlConfigWatcher: Watches a directory for YAML config changes, additions, deletions.
// Keeps ExperienceManager and clients in sync with the current YAML configuration state.

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using YamlConfigurations;
using YamlConfigurations.FileReader;
using WebSocketMessages.Messages;
using AppExtensions.Experience;
using WebSocketMessages.Messages.Rooms;
using WebSocketMessages;

namespace AppExtensions.Watcher
{
    /// <summary>
    /// Notifies all WebSocket clients about config reload events.
    /// </summary>
    public class ConfigReloadNotifier : IConfigReloadNotifier
    {
        private readonly WebSocketHandler _handler;
        public ConfigReloadNotifier(WebSocketHandler handler) => _handler = handler;
        public void NotifyConfigReload(WebSocketConfigReloadedMessage message) => _handler.SendToAllClients(message);
    }

    /// <summary>
    /// Watches a directory for any YAML config changes (add/update/delete/rename).
    /// Updates ExperienceManager and notifies clients accordingly.
    /// </summary>
    public class YamlConfigWatcher
    {
        private readonly FileSystemWatcher _directoryWatcher;
        private readonly SemaphoreSlim _reloadLock = new(1, 1);
        private readonly ExperienceManager _experienceManager;
        private readonly IConfigReloadNotifier _notifier;
        private Dictionary<string, YamlMultipleChatRooms> _yamlConfig = new();
        private readonly Dictionary<string, Timer> _debounceTimers = new();

        public event Action<Dictionary<string, YamlMultipleChatRooms>>? OnConfigReloaded;
        public Func<bool>? IsProcessingWebSocketMessageFunc { get; set; }

        public YamlConfigWatcher(string agentsDirectory, ExperienceManager experienceManager, WebSocketHandler webSocketHandler)
        {
            _experienceManager = experienceManager;
            _notifier = new ConfigReloadNotifier(webSocketHandler);

            // Initial load of all existing YAML configs from ExperienceManager
            var initialConfig = new Dictionary<string, YamlMultipleChatRooms>();
            foreach (var kvp in experienceManager.Experiences)
            {
                if (kvp.Value?.Experience != null)
                {
                    initialConfig[kvp.Key] = kvp.Value.Experience;
                }
            }

            // Watch the directory for any YAML changes/additions/deletions/renames
            _directoryWatcher = new FileSystemWatcher(agentsDirectory)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Size,
                Filter = "*.*"
            };
            _directoryWatcher.Changed += OnFileSystemEvent;
            _directoryWatcher.Created += OnFileSystemEvent;
            _directoryWatcher.Deleted += OnFileSystemEvent;
            _directoryWatcher.Renamed += OnRenamedEvent;
            _directoryWatcher.EnableRaisingEvents = true;
        }

        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            if (!IsYaml(e.FullPath))
                return;

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    SafeReload(e.FullPath, "configAdded");
                    break;
                case WatcherChangeTypes.Changed:
                    SafeReload(e.FullPath, "configReloaded");
                    break;
                case WatcherChangeTypes.Deleted:
                    SafeDelete(e.FullPath);
                    break;
            }
        }

        private void OnRenamedEvent(object sender, RenamedEventArgs e)
        {
            SafeReload(e.FullPath, "configReloaded");

        }

        private void SafeReload(string path, string action)
        {
            if (!IsYaml(path) || !File.Exists(path))
                return;

            Debounce(path, async () =>
            {
                // Wait for any in-flight WebSocket processing
                while (IsProcessingWebSocketMessageFunc != null && IsProcessingWebSocketMessageFunc())
                    Thread.Sleep(50);

                Dictionary<string, YamlMultipleChatRooms> newConfig;
                try
                {
                    (_, _, newConfig) = YamlFileReader.ReadFile(path);
                }
                catch
                {
                    // parsing error; ignore
                    return;
                }

                if (newConfig.Count == 0)
                    return;

                await _reloadLock.WaitAsync();
                try
                {
                    // Merge newConfig into _yamlConfig (update or add)
                    foreach (var kvp in newConfig)
                        _yamlConfig[kvp.Key] = kvp.Value;

                    OnConfigReloaded?.Invoke(_yamlConfig);
                    await UpdateExperienceManagerAsync(_yamlConfig);
                    NotifyClients(newConfig, action); // still notify only about the changed file
                }
                finally
                {
                    _reloadLock.Release();
                }
            });
        }

        private void SafeDelete(string path)
        {
            if (!IsYaml(path))
                return;

            var roomKey = Path.GetFileNameWithoutExtension(path);
            _reloadLock.Wait();
            try
            {
                _yamlConfig.Remove(roomKey);
                _experienceManager.Experiences.Remove(roomKey);
                OnConfigReloaded?.Invoke(_yamlConfig);
                NotifyClients(new Dictionary<string, YamlMultipleChatRooms> { { roomKey, null! } }, "configRemoved");
            }
            finally
            {
                _reloadLock.Release();
            }
        }

        private static bool IsYaml(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            return ext == ".yml" || ext == ".yaml";
        }

        private void Debounce(string key, Action work)
        {
            if (_debounceTimers.TryGetValue(key, out var timer))
            {
                timer.Change(200, Timeout.Infinite);
            }
            else
            {
                timer = new Timer(_ => work(), null, 200, Timeout.Infinite);
                _debounceTimers[key] = timer;
            }
        }

        private async Task UpdateExperienceManagerAsync(Dictionary<string, YamlMultipleChatRooms> config)
        {
            await _experienceManager.UpdateFromConfigAsync(config);

        }

        private void NotifyClients(Dictionary<string, YamlMultipleChatRooms> config, string action)
        {
            foreach (var kvp in config)
            {

                var value = kvp.Value;

                List<WebSocketValidationError> wsErrors = value.Errors?
                  .Select(e => new WebSocketValidationError
                  {
                      Message = e.Message,

                      LineNumber = e.LineNumber,
                      CharPosition = e.CharPosition
                  }).ToList() ?? new List<WebSocketValidationError>();


                var wsRoom = new WebSocketGetRooms
                {
                    Name = value?.Name ?? string.Empty,
                    DisplayName = value?.DisplayName ?? string.Empty,
                    Emoji = value?.Emoji ?? string.Empty,
                    AutoStart = value?.AutoStart ?? string.Empty,
                    Yaml = value?.Yaml ?? string.Empty,
                    Errors = wsErrors,
                    UserId = "system",
                    Rooms = value?.Rooms != null
                        ? [.. value.Rooms.Values.Select(room => new WebSocketRoomProfile
                        {
                            Name = room.Name,
                            DisplayName = room.DisplayName,
                            Emoji = room.Emoji,
                            Agents = room.Agents?.Select(agent => new WebSocketAgentProfile
                            {
                                Name = agent.Name,
                                DisplayName = agent.DisplayName,
                                Emoji = agent.Emoji ?? ""
                            }).ToList() ?? []
                        })]
                        : []
                };
                var msg = new WebSocketConfigReloadedMessage
                {
                    ChangedRoom = wsRoom,
                    Action = action
                };
                _notifier.NotifyConfigReload(msg);
            }
        }

        /// <summary>
        /// Returns the current YAML config.
        /// </summary>
        public Dictionary<string, YamlMultipleChatRooms> GetCurrentConfig() => _yamlConfig;
    }
}
