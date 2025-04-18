

using api.src;
using AppExtensions.Experience;
using AppExtensions.SemanticKernel;
using AppExtensions.Logging;
using Microsoft.SemanticKernel;
using WebSocketMessages;
using WebSocketMessages.AgentLifecycle;

WebApplicationBuilder builder = SetupBuilder(args);

var app = builder.Build();

// Enable CORS
app.UseCors("AllowFrontend");

// Create WebSocketHandler
var webSocketHandler = new WebSocketHandler();

var configBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("localsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Environment variables take precedence

IConfiguration configuration = configBuilder.Build();

// Initialize KernelHandler
var setupForLlmRequested = configuration["LlmSetup"] ?? "Ollama";
var setupForVectorDBRequested = configuration["VectorSetup"] ?? "Qdrant";

Kernel kernel = SetupKernel(configuration, setupForLlmRequested);


ExperienceManager manager = new(kernel);

// Determine the base directory and the Agents directory
string baseDirectory = AppContext.BaseDirectory;
string agentsDirectory = Path.Combine(baseDirectory, "Agents");
 

bool resultOfAction;
resultOfAction = await manager.ReadDirectoryAsync(agentsDirectory);
resultOfAction = await manager.CreateOrchestratorsAsync();


manager.RegisterHandlers(webSocketHandler);

AppExtensions.Experience.Handlers.LibrarianHandler.EmbeddingDimension = KernelHelper.EmbeddingDimension;

//if using the editor, we need this for more information.
app.UseWebSockets();


var webSocketAgentLifecycleSender = new WebSocketAgentLifecycleSender("editor-service");



// Map the "/ws" endpoint for WebSocket connections.
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


app.Run();

static Kernel SetupKernel(IConfiguration configuration, string setupForLlmRequested)
{
    // Create the kernel builder for initializing services.
    var kernelBuilder = Kernel.CreateBuilder();

    if (setupForLlmRequested == "Ollama")
    {
        KernelHelper.SetupOllama(kernelBuilder, configuration);
    }
    else
    {
        KernelHelper.SetupAzure(kernelBuilder, configuration);
    }

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