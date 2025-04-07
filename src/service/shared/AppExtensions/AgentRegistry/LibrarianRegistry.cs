using api.src.SemanticKernel.VectorStore;

using Microsoft.SemanticKernel;


using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using YamlConfigurations.Librarians;
using WebSocketMessages.Messages;
using WebSocketMessages;
using WebSocketMessages.Messages.Librarians;
using AppExtensions.SemanticKernel.VectorStore;


namespace AppExtensions.AgentRegistry
{
    public class LibrarianRegistry
    {
        // Dictionary to hold librarians data.
        private readonly Dictionary<string, YamLibrarians> dictLibrarians = new();
        static public int EmbeddingDimension = 0;
        /// <summary>
        /// Appends additional librarians groups to the registry.
        /// </summary>
        public void AppendLibrarians(Dictionary<string, YamLibrarians> moreLibrarians)
        {
            foreach (var kvp in moreLibrarians)
            {
                if (!dictLibrarians.ContainsKey(kvp.Key))
                {
                    dictLibrarians.Add(kvp.Key, kvp.Value);
                }
            }
        }

        /// <summary>
        /// Handles the "librarians" command by checking the SubAction and dispatching
        /// to the appropriate method.
        /// </summary>
        public async Task HandleLibrariansCommandAsync(WebSocketBaseMessage message, WebSocket socket,  ConnectionMode mode)
        {
            if (message.SubAction == "get")
            {
                await HandleGetLibrarians(message, socket);
            }
            else if (message.SubAction == "converse")
            {
               // await HandleConverseWithLibrary(message, socket, kernel);
            }
            else if (message.SubAction == "list")
            {
              //  await HandleListWithLibrary(message, socket, kernel);
            }
            else if (message.SubAction == "docs")
            {
              // await HandleDocRequestWithLibrary(message, socket, kernel);
            }
        }



        /// <summary>
        /// Returns a list of librarians in a JSON response.
        /// </summary>
        private async Task HandleGetLibrarians(WebSocketBaseMessage message, WebSocket socket)
        {
            var response = new WebSocketGetLibrarians
            {
                UserId = "system",
                TransactionId = Guid.NewGuid().ToString(),
                Action = "librarians",
                SubAction = "list",
            };

            // Iterate through each librarians group in the registry.
            foreach (var (_, librariansGroup) in dictLibrarians)
            {
                if (librariansGroup.ActiveLibrarians.Count + librariansGroup.NotActiveLibrarians.Count <= 0)
                {
                    continue;
                }

                // Create a room profile for this librarians group.
                var roomProfile = new WebSocketLibraryRoomProfile
                {
                    Name = librariansGroup.RoomName,
                    Emoji = librariansGroup.RoomEmoji,
                    ActiveLibrarians = new List<WebSocketLibrarianProfile>(),
                    NotActiveLibrarians = new List<WebSocketLibrarianProfile>()
                };

                // Map each active librarian.
                foreach (var librarian in librariansGroup.ActiveLibrarians)
                {
                    roomProfile.ActiveLibrarians.Add(new WebSocketLibrarianProfile
                    {
                        Name = librarian.Name,
                        Emoji = librarian.Emoji ?? ""
                    });
                }

                // Map each not-active librarian.
                foreach (var librarian in librariansGroup.NotActiveLibrarians)
                {
                    roomProfile.NotActiveLibrarians.Add(new WebSocketLibrarianProfile
                    {
                        Name = librarian.Name,
                        Emoji = librarian.Emoji  ?? ""
                    });
                }


                response.Rooms.Add(roomProfile);
            }

            var responseJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            await socket.SendAsync(new ArraySegment<byte>(responseJson), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Handles the "converse" command for librarians by extracting the payload and
        /// finding the matching active librarian.
        /// </summary>
        private async Task HandleConverseWithLibrary(WebSocketBaseMessage message, WebSocket socket, Kernel kernel)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                await SendErrorAsync(socket, "converse", "Invalid converse payload");
                return;
            }

            JsonContentPlayLoadForLibrarian? payload;
            try
            {
                // Use case-insensitive deserialization options.
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                payload = JsonSerializer.Deserialize<JsonContentPlayLoadForLibrarian>(message.Content, options);

                if (payload == null || string.IsNullOrWhiteSpace(payload.Text))
                {
                    await SendErrorAsync(socket, "converse", "Invalid converse payload");
                    return;
                }
            }
            catch (Exception ex)
            {
                await SendErrorAsync(socket, "converse", $"Failed to parse converse payload: {ex.Message}");
                return;
            }

            // Use LINQ to find the active librarian matching room and agent names.
            var foundAgent = dictLibrarians.Values
                .Where(lib => lib.RoomName.Equals(payload.RoomName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(lib => lib.ActiveLibrarians)
                .FirstOrDefault(agent => agent.Name.Equals(payload.AgentName, StringComparison.OrdinalIgnoreCase));

            if (foundAgent == null)
            {
                await SendErrorAsync(socket, "converse", $"Librarian '{payload.AgentName}' in room '{payload.RoomName}' not found among active librarians");
                return;
            }

            var socketMessage = new WebSocketLibrarianConverse
            {
                Action = "librarian",
                UserId = foundAgent.Name,
                TransactionId = Guid.NewGuid().ToString(),
                SubAction = "converse-message",
                Question = payload.Text,
                AgentName = foundAgent.Name,
                RoomName = payload.RoomName
            };

            var optionsJsonWrite = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

            //Get the vector store and usse the text to just get related documents to the text in a list
            /* TODO ON UPGRADE
                        await foreach (var (finalPrompt, decisionResult, decisionThinking) in
                              YamlHelpers.GetDecisionAsync(foundAgent.Instructions, payload.Text, kernel))
                        {

                            socketMessage.Content = decisionResult;
                            socketMessage.Thinking = decisionThinking;
                            string json = JsonSerializer.Serialize(socketMessage, optionsJsonWrite);
                            // Convert the JSON string to UTF8-encoded bytes.
                            var bytes = Encoding.UTF8.GetBytes(json);


                            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
            */

            // Process the conversation message as needed.
            Console.WriteLine($"Received converse message from {payload.AgentName} in room {payload.RoomName}: {payload.Text}");

            // Process the conversation message as needed.
            Console.WriteLine($"Received converse message from {payload.AgentName} in room {payload.RoomName}: {payload.Text}");

        }

        private async Task HandleListWithLibrary(WebSocketBaseMessage message, WebSocket socket, Kernel kernel)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                await SendErrorAsync(socket, "list", "Invalid converse payload");
                return;
            }

            JsonContentPlayLoadForLibrarian? payload;
            try
            {
                // Use case-insensitive deserialization options.
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                payload = JsonSerializer.Deserialize<JsonContentPlayLoadForLibrarian>(message.Content, options);

                if (payload == null || string.IsNullOrWhiteSpace(payload.Text))
                {
                    await SendErrorAsync(socket, "list", "Invalid converse payload");
                    return;
                }
            }
            catch (Exception ex)
            {
                await SendErrorAsync(socket, "list", $"Failed to parse converse payload: {ex.Message}");
                return;
            }

            // Use LINQ to find the active librarian matching room and agent names.
            var foundAgent = dictLibrarians.Values
                .Where(lib => lib.RoomName.Equals(payload.RoomName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(lib => lib.ActiveLibrarians)
                .FirstOrDefault(agent => agent.Name.Equals(payload.AgentName, StringComparison.OrdinalIgnoreCase));

            if (foundAgent == null)
            {
                await SendErrorAsync(socket, "list", $"Librarian '{payload.AgentName}' in room '{payload.RoomName}' not found among active librarians");
                return;
            }

            if (foundAgent.Collection == null || string.IsNullOrEmpty(foundAgent.Collection.Name))
            {
                await SendErrorAsync(socket, "list", $"Librarian '{payload.AgentName}' in room '{payload.RoomName}' no collection");
                return;
            }

            var agentCollection = foundAgent.Collection;

            var socketMessage = new WebSocketLibrarianList
            {
                Action = "librarian",
                UserId = foundAgent.Name,
                TransactionId = Guid.NewGuid().ToString(),
                SubAction = "list",
                Question = payload.Text,
                AgentName = foundAgent.Name,
                RoomName = payload.RoomName,
                References = []
            };

            var optionsJsonWrite = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

         
     
            if (EmbeddingDimension == 3584)
            {
                await foreach (var document in VectorStoreHelper<TextParagraphEmbeddingOf3584>.GetRelatedDocuments(kernel, agentCollection,  payload.Text, 5))
                {
                    WebSocektLibrainDocRef reference = new()
                    {
                        Text = document.Record.Text,
                        Score = document.Score.ToString(),
                        DocumentUri = document.Record.DocumentUri,
                        Question = document.Record.Question,
                    };

                    socketMessage.References.Add(reference);
                    string json = JsonSerializer.Serialize(socketMessage, optionsJsonWrite);
                    // Convert the JSON string to UTF8-encoded bytes.
                    var bytes = Encoding.UTF8.GetBytes(json);

                    await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            else if (EmbeddingDimension == 1536)
            {
                await foreach (var document in VectorStoreHelper<TextParagraphEmbeddingOf3584>.GetRelatedDocuments(kernel, agentCollection, payload.Text, 5))
                {
                    WebSocektLibrainDocRef reference = new()
                    {
                        Text = document.Record.Text,
                        Score = document.Score.ToString(),
                        DocumentUri = document.Record.DocumentUri,
                        Question = document.Record.Question,
                    };
                    
                    socketMessage.References.Add(reference);

                    string json = JsonSerializer.Serialize(socketMessage, optionsJsonWrite);
                    // Convert the JSON string to UTF8-encoded bytes.
                    var bytes = Encoding.UTF8.GetBytes(json);

                    await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }


        private async Task HandleDocRequestWithLibrary(WebSocketBaseMessage message, WebSocket socket, Kernel kernel)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                await SendErrorAsync(socket, "doc", "Invalid converse payload");
                return;
            }

            JsonContentPlayLoadForGetDocsInLibrarian? payload;
            try
            {
                // Use case-insensitive deserialization options.
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                payload = JsonSerializer.Deserialize<JsonContentPlayLoadForGetDocsInLibrarian>(message.Content, options);

                if (payload == null)
                {
                    await SendErrorAsync(socket, "doc", "Invalid converse payload");
                    return;
                }

                payload.Top = Math.Max(1, payload.Top);
                payload.Skip = Math.Max(0, payload.Top);
            }
            catch (Exception ex)
            {
                await SendErrorAsync(socket, "doc", $"Failed to parse converse payload: {ex.Message}");
                return;
            }

            // Use LINQ to find the active librarian matching room and agent names.
            var foundAgent = dictLibrarians.Values
                .Where(lib => lib.RoomName.Equals(payload.RoomName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(lib => lib.ActiveLibrarians)
                .FirstOrDefault(agent => agent.Name.Equals(payload.AgentName, StringComparison.OrdinalIgnoreCase));

            if (foundAgent == null)
            {
                await SendErrorAsync(socket, "doc", $"Librarian '{payload.AgentName}' in room '{payload.RoomName}' not found among active librarians");
                return;
            }

            if (foundAgent.Collection == null || string.IsNullOrEmpty(foundAgent.Collection.Name))
            {
                await SendErrorAsync(socket, "doc", $"Librarian '{payload.AgentName}' in room '{payload.RoomName}' no collection");
                return;
            }

            var agentCollection = foundAgent.Collection;

            var socketMessage = new WebSocketLibrarianList
            {
                Action = "librarian",
                UserId = foundAgent.Name,
                TransactionId = Guid.NewGuid().ToString(),
                SubAction = "doc",
                Question = $"Top {payload.Top} and Skip {payload.Skip}",
                AgentName = foundAgent.Name,
                RoomName = payload.RoomName,
                References = []
            };

            var optionsJsonWrite = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

            try
            {

                if (EmbeddingDimension == 3584)
                {
                    await foreach (var document in VectorStoreHelper<TextParagraphEmbeddingOf3584>.GetDocuments(kernel, agentCollection, payload.Top, payload.Skip))
                    {
                        WebSocektLibrainDocRef reference = new()
                        {
                            Text = document.Record.Text,
                            Score = document.Score.ToString(),
                            DocumentUri = document.Record.DocumentUri,
                            Question = document.Record.Question,
                        };

                        socketMessage.References.Add(reference);
                        string json = JsonSerializer.Serialize(socketMessage, optionsJsonWrite);
                        // Convert the JSON string to UTF8-encoded bytes.
                        var bytes = Encoding.UTF8.GetBytes(json);

                        await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else if (EmbeddingDimension == 1536)
                {
                    await foreach (var document in VectorStoreHelper<TextParagraphEmbeddingOf3584>.GetDocuments(kernel, agentCollection, payload.Top, payload.Skip))
                    {
                        WebSocektLibrainDocRef reference = new()
                        {
                            Text = document.Record.Text,
                            Score = document.Score.ToString(),
                            DocumentUri = document.Record.DocumentUri,
                            Question = document.Record.Question,
                        };

                        socketMessage.References.Add(reference);

                        string json = JsonSerializer.Serialize(socketMessage, optionsJsonWrite);
                        // Convert the JSON string to UTF8-encoded bytes.
                        var bytes = Encoding.UTF8.GetBytes(json);

                        await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Sends an error message over the provided WebSocket.
        /// </summary>
        private async Task SendErrorAsync(WebSocket webSocket, string subAction, string errorMessage)
        {
            var errorResponse = new WebSocketBaseMessage
            {
                Action = "error",
                SubAction = subAction,
                Content = errorMessage
            };

            var errorJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorResponse));
            await webSocket.SendAsync(new ArraySegment<byte>(errorJson), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
