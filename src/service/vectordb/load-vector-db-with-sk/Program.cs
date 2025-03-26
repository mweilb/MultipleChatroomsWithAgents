 
using LoadVectorDbWithSk;
using Microsoft.SemanticKernel;
using MultiAgents.SemanticKernel;
using MultiAgents.SemanticKernel.VectorStore;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var builder = WebApplication.CreateBuilder(args);

// Build configuration including appsettings.json and appsettings.Development.json.
var configBuilder = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Logging.SetMinimumLevel(LogLevel.Trace);

IConfiguration configuration = configBuilder.Build();

// Create the kernel builder for initializing services.
var kernelBuilder = Kernel.CreateBuilder();

// Initialize KernelHandler (using Ollama and Qdrant as in your example).

KernelHelper.SetupOllama(kernelBuilder, configuration);
// KernelHelper.SetupAzure(kernelBuilder, configuration);


KernelHelper.SetupQdrant(kernelBuilder, configuration);
// KernelHelper.SetupPinecone(kernelBuilder, configuration);
// KernelHelper.SetupAzureSearch(kernelBuilder, configuration);

// Build the kernel.
var kernel = kernelBuilder.Build();

// Get the definitions folder path. (Assuming it's inside your content root.)
string baseDirectory = AppContext.BaseDirectory;
string definitionsFolder = Path.Combine(baseDirectory, "Definitions");
 
if (!Directory.Exists(definitionsFolder))
{
    Console.WriteLine($"Definitions folder does not exist: {definitionsFolder}");
}
else
{
    // Get all YAML files (both .yaml and .yml) in the definitions folder and its subdirectories.
    var yamlFiles = Directory.GetFiles(definitionsFolder, "*.yaml", SearchOption.AllDirectories)
        .Concat(Directory.GetFiles(definitionsFolder, "*.yml", SearchOption.AllDirectories));

    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    var allCollections = new List<CollectionDefinition>();
    foreach (var file in yamlFiles)
    {
        Console.WriteLine($"Loading definitions from: {file}");
        string yamlContent = await File.ReadAllTextAsync(file);
        var config = deserializer.Deserialize<CollectionsConfiguration>(yamlContent);
        if (config?.Collections != null)
        {
            allCollections.AddRange(config.Collections);
        }
    }

    // Process each collection definition.
    foreach (var collDef in allCollections)
    {
        Console.WriteLine($"Processing collection: {collDef.Name}");
        // Validate the collection name: it must contain only letters and numbers.
        // Adjust the regex if you want to allow additional characters (like underscores).
        if (!RegexCollectionValidate().IsMatch(collDef.Name))
        {
            string fixedName = RegexCollection().Replace(collDef.Name, "");
            Console.WriteLine($"Fixed collection name: '{fixedName}' (original was '{collDef.Name}').");
            collDef.Name = fixedName;
        }
        if (KernelHelper.EmbeddingDimension == 3584)
        {
            // Call your DirectoryLoader generic method.
            // Here we assume you're using TextParagraphEmbeddingOf3584 as the model type.
            await DirectoryLoader<TextParagraphEmbeddingOf3584>.LoadFilesFromDirectory(
                collDef.SearchDirectory,
                collDef.FileFilters,
                collDef.DirectoriesToIgnore,
                collDef.Questions,
                kernel,
                collDef.Name
                 
            );
        }
        else if (KernelHelper.EmbeddingDimension == 1536)
        {
            // Call your DirectoryLoader generic method.
            // Here we assume you're using TextParagraphEmbeddingOf3584 as the model type.
            await DirectoryLoader<TextParagraphEmbeddingOf1536>.LoadFilesFromDirectory(
                collDef.SearchDirectory,
                collDef.FileFilters,
                collDef.DirectoriesToIgnore,
                collDef.Questions,
                kernel,
                collDef.Name
               );
        }
    }
}
partial class Program
{
    [GeneratedRegex("[^A-Za-z0-9]")]
    private static partial Regex RegexCollection();
    [GeneratedRegex("^[A-Za-z0-9]+$")]
    private static partial Regex RegexCollectionValidate();
}
 