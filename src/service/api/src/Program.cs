﻿﻿﻿﻿// Program.cs - Main entry point for the API service

using api.src;
using AppExtensions;
using AppExtensions.Experience;
using AppExtensions.Logging;
using AppExtensions.SemanticKernel;
using Microsoft.SemanticKernel;
using WebSocketMessages;
using WebSocketMessages.AgentLifecycle;
using WebSocketMessages.Messages;

// Build the web application and configure services
WebApplicationBuilder builder = SetupBuilder(args);

var app = builder.Build();

// Enable CORS for frontend access
app.UseCors("AllowFrontend");

// Create the WebSocket handler for managing WebSocket connections
var webSocketHandler = new WebSocketHandler();

// Build configuration from appsettings and environment variables
var configBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddJsonFile("localsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Environment variables take precedence

IConfiguration configuration = configBuilder.Build();

// Initialize LLM and vector DB setup options from configuration
var setupForLlmRequested = configuration.GetValue<string>("LlmSetup", "Ollama");
var setupForVectorDBRequested = configuration.GetValue<string>("VectorSetup", "Qdrant");

// Create the Semantic Kernel instance
Kernel kernel = SetupKernel(configuration, setupForLlmRequested);

// ExperienceManager manages agent experiences and orchestrators
ExperienceManager manager = new(kernel);

// Determine the base directory and the Agents directory
string baseDirectory = AppContext.BaseDirectory;
string agentsDirectory = Path.Combine(baseDirectory, "Agents");

// Override agentsDirectory if DevelopermentLocation is set in configuration
var devLocation = configuration["DevelopermentLocation"];
if (!string.IsNullOrEmpty(devLocation))
{
    agentsDirectory = devLocation;
}

// (Legacy) Get all YAML files in the Agents directory (now handled by YamlConfigWatcher)
var yamlFiles = Directory.Exists(agentsDirectory)
    ? Directory.GetFiles(agentsDirectory, "*.yml", SearchOption.AllDirectories).ToList()
    : new List<string>();

// Load agent YAML files and create orchestrators
bool resultOfAction;
resultOfAction = await manager.ReadDirectoryAsync(agentsDirectory);
resultOfAction = await manager.CreateOrchestratorsAsync();

// Register WebSocket command handlers for agent experiences
manager.RegisterHandlers(webSocketHandler);

// Set embedding dimension for the librarian handler
AppExtensions.Experience.Handlers.LibrarianHandler.EmbeddingDimension = KernelHelper.EmbeddingDimension;

// Enable WebSocket support for the app
app.UseWebSockets();

// Create a sender for agent lifecycle events over WebSocket
var webSocketAgentLifecycleSender = new WebSocketAgentLifecycleSender("editor-service");

// Create the YAML config watcher, which monitors YAML config changes in the agents directory
// Create the YAML config watcher, which monitors YAML config changes in the agents directory
var yamlConfigWatcher = new AppExtensions.Watcher.YamlConfigWatcher(agentsDirectory, manager, webSocketHandler);

// Directory-based watcher for appsettings.Development.json changes
string configDir = Path.GetDirectoryName(Path.Combine(baseDirectory, "appsettings.Development.json")) ?? baseDirectory;
string configFile = Path.Combine(configDir, "appsettings.Development.json");
DateTime lastWriteTime = File.Exists(configFile) ? File.GetLastWriteTimeUtc(configFile) : DateTime.MinValue;
string? lastDevLocation = devLocation;

var timer = new System.Threading.Timer(_ =>
{
    DateTime currentWriteTime = File.Exists(configFile) ? File.GetLastWriteTimeUtc(configFile) : DateTime.MinValue;
    if (currentWriteTime != lastWriteTime)
    {
        lastWriteTime = currentWriteTime;
        var newConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddJsonFile("localsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var newDevLocation = newConfig["DevelopermentLocation"];
        if (!string.IsNullOrEmpty(newDevLocation))
        {
            lastDevLocation = newDevLocation;
            agentsDirectory = newDevLocation;
            yamlConfigWatcher = new AppExtensions.Watcher.YamlConfigWatcher(agentsDirectory, manager, webSocketHandler);
        }
    }
}, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

// Map the "/ws" endpoint for WebSocket connections
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine("WebSocket connection established");
        webSocketAgentLifecycleSender.WebSocket = webSocket;
        webSocketAgentLifecycleSender.CurrentConnectionMode = webSocketHandler.CurrentConnectionMode;
        await webSocketHandler.HandleRequestAsync(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// Start the web application
app.Run();

/// <summary>
/// Sets up the Semantic Kernel with LLM and vector DB providers.
/// </summary>
static Kernel SetupKernel(IConfiguration configuration, string setupForLlmRequested)
{
    // Create the kernel builder for initializing services.
    var kernelBuilder = Kernel.CreateBuilder();

    // Configure LLM provider
    if (setupForLlmRequested == "Ollama")
    {
        KernelHelper.SetupOllama(kernelBuilder, configuration);
    }
    else
    {
        KernelHelper.SetupAzure(kernelBuilder, configuration);
    }

    // Configure vector DB provider
    if (setupForLlmRequested == "Qdrant")
    {
        KernelHelper.SetupQdrant(kernelBuilder, configuration);
    }
    else if (setupForLlmRequested == "Pinecone")
    {
        KernelHelper.SetupPinecone(kernelBuilder, configuration);
    }
    else if (setupForLlmRequested == "AzureSearch")
    {
        KernelHelper.SetupAzureSearch(kernelBuilder, configuration);
    }

    // Add logging to the kernel
    kernelBuilder.Services.AddLogging();

    // Create a base logger factory with a built‐in provider (like Console).
    var baseLoggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Trace);
    });

    // Create your custom provider with the base factory.
    var listeningProvider = new ListeningLoggerProvider(baseLoggerFactory);

    // Build a logger factory that uses your custom provider.
    var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.ClearProviders(); // Clear default providers
        builder.AddProvider(listeningProvider);
        builder.SetMinimumLevel(LogLevel.Trace);
    });

    // Register the logger factory in the DI container.
    kernelBuilder.Services.AddSingleton<ILoggerFactory>(loggerFactory);

    var kernel = kernelBuilder.Build();

    return kernel;
}

/// <summary>
/// Sets up the web application builder and configures services and CORS.
/// </summary>
static WebApplicationBuilder SetupBuilder(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllers();

    // Configure CORS to allow the React app (assumes it runs on http://localhost:3000)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });

    });
    return builder;
}
