

using api.src;
using Microsoft.SemanticKernel;
using multi_agents_shared.src.AISpeech;
using MultiAgents.AgentsChatRoom.AgentRegistry;
using MultiAgents.AzureAISpeech;
using MultiAgents.SemanticKernel;
using MultiAgents.WebSockets;



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

var app = builder.Build();

var configBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("localsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Environment variables take precedence

IConfiguration configuration = configBuilder.Build();


// Create the kernel builder for initializing services.
var kernelBuilder = Kernel.CreateBuilder();

// Initialize KernelHandler

//KernelHelper.SetupAzure(kernelBuilder, configuration);
KernelHelper.SetupOllama(kernelBuilder, configuration);

KernelHelper.SetupQdrant(kernelBuilder, configuration);
//KernelHelper.SetupPinecone(kernelBuilder, configuration);
//KernelHelper.SetupAzureSearch(kernelBuilder, configuration);
// Build the kernel.
var kernel = kernelBuilder.Build();

// Enable CORS
app.UseCors("AllowFrontend");

// Create WebSocketHandler
var webSocketHandler = new WebSocketHandler();


AgentRoomRegistry manager = new();


// Determine the base directory and the Agents directory
string baseDirectory = AppContext.BaseDirectory;
string agentsDirectory = Path.Combine(baseDirectory, "Agents");
string expereincesDirectory = Path.Combine(baseDirectory, "Experiences");
//hack to get the reading right..

var (rooms, librariesRoom) = await RegisterYamls.RegisterSingleRoomsAsync(kernel, KernelHelper.EmbeddingDimension, webSocketHandler, agentsDirectory);

manager.AppendRooms(rooms);
manager.AppendLibrarians(librariesRoom);

var (multiRooms, librariesMultiRooms) = await RegisterYamls.RegisterMultiRoomsAsync(kernel, KernelHelper.EmbeddingDimension, webSocketHandler, expereincesDirectory);
manager.AppendRooms(multiRooms);
manager.AppendLibrarians(librariesRoom);
 
//can also do code version..
//agentHandlerManager.AddAgentChatRoom(new ExampleAgentRegistry(), new ExampleAgentHandler(), kernel);

manager.RegisterHandlers(webSocketHandler);

//Now handle the speech part
IAgentSpeech agentSpeech = new AzureAgentSpeech();
if (agentSpeech.Initialize(configuration) == false)
{
    agentSpeech = new NullAgentSpeech();
}

AiSpeechActiveHandler speechHandler = new(webSocketHandler, agentSpeech);

LibrarianRegistry.EmbeddingDimension = KernelHelper.EmbeddingDimension;

app.UseWebSockets();

// Map the "/ws" endpoint for WebSocket connections.
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine("WebSocket connection established");
        await webSocketHandler.HandleRequestAsync(webSocket, kernel, agentSpeech);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});


app.Run();
