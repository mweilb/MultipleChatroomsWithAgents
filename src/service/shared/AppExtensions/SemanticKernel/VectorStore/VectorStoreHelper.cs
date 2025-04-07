
using AppExtensions.SemanticKernel.VectorStore;
using AppExtensions.SemanticKernel.VectorStore.Documents;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.Text.RegularExpressions;
using YamlConfigurations;

#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace api.src.SemanticKernel.VectorStore
{
    /// <summary>
    /// A generic helper class for uploading documents to the vector store.
    /// The type parameter T allows you to specify which TextParagraph-derived type (and thus embedding dimension) to use.
    /// </summary>
    /// <typeparam name="T">A type that inherits from TextParagraph and has a parameterless constructor.</typeparam>
    public partial class VectorStoreHelper<T> where T : TextParagraph, new()
    {
        /// <summary>
        /// Private helper method that processes each question:
        /// it invokes a chat prompt using the full document text and the question,
        /// then uploads the resulting embedding along with the original document.
        /// </summary>
        /// <param name="documentIdentifier">A unique identifier for the document (e.g. file path or URL).</param>
        /// <param name="fullText">The full text of the document.</param>
        /// <param name="kernel">The Semantic Kernel instance.</param>
        /// <param name="collectionName">The vector store collection name.</param>
        /// <param name="questions">Array of questions to ask.</param>
        private static async Task ProcessQuestionsAndUpload(
            string documentIdentifier,
            string fullText,
            Kernel kernel,
            string collectionName,
            string[] questions)
        {
            var dataUploader = kernel.Services.GetRequiredService<DataUploader>();

            // Define a prompt template that uses the document and a question.
            string promptTemplate = "Document:\n{{document}}\n\nQuestion: {{question}}\nAnswer:";
            var promptTemplateFactory = new HandlebarsPromptTemplateFactory();
            int idxQuestion = 0;
            foreach (var question in questions)
            {
                idxQuestion++;
                // Create kernel arguments with the full text and current question.
                var arguments = new KernelArguments
                {
                    { "document", fullText },
                    { "question", question }
                };

                // Invoke the chat prompt asynchronously.
                var response = await kernel.InvokePromptAsync(
                    promptTemplate,
                    arguments,
                    templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                    promptTemplateFactory: promptTemplateFactory
                );

                // Extract the answer from the Variables collection.
                string embeddedText = response.GetValue<string>() ?? "";

  

                if (!string.IsNullOrWhiteSpace(embeddedText))
                {
                    // Upload an embedding entry for this question.
                    await dataUploader.GenerateEmbeddingsAndUploadAsync<T>(
                        collectionName,
                        documentIdentifier,
                        "question " + idxQuestion,
                        question,
                        embeddedText,
                        fullText);
                }
            }

            // Finally, upload the full (raw) document as well.
            await dataUploader.GenerateEmbeddingsAndUploadAsync<T>(
                collectionName,
                documentIdentifier,
                "full",
                "raw",
                null,
                fullText);
        }

        /// <summary>
        /// Splits a PDF into chunks and uploads it to the vector store.
        /// Uses the document’s full text to process questions.
        /// </summary>
        public static async Task SavePdfToVectorStore(string pdfPath, Kernel kernel, string collectionName, string[] questions)
        {
            // Read full text from PDF.
            (_, var fullText) = PdfReader.ReadText(pdfPath);
            await ProcessQuestionsAndUpload(pdfPath, fullText, kernel, collectionName, questions);
        }

        /// <summary>
        /// Splits a text file into chunks and uploads it to the vector store.
        /// Uses the document’s full text to process questions.
        /// </summary>
        public static async Task SaveTextToVectorStore(string textPath, Kernel kernel, string collectionName, string[] questions)
        {
            // Read full text from the text file.
            (_, var fullText) = TextFileReader.ReadText(textPath);
            await ProcessQuestionsAndUpload(textPath, fullText, kernel, collectionName, questions);
        }

        /// <summary>
        /// Reads a Word document and uploads it to the vector store.
        /// Uses the document’s full text to process questions.
        /// </summary>
        public static async Task SaveWordToVectorStore(string url, string path, Kernel kernel, string collectionName, string[] questions)
        {
            // Read full text from the Word document.
            (_, var fullText) = DocumentReader.ReadText(new FileStream(path, FileMode.Open), url);
            await ProcessQuestionsAndUpload(url, fullText, kernel, collectionName, questions);
        }

        /// <summary>
        /// Adds a text search plugin to the kernel based on the collection configuration.
        /// It validates the configuration, ensuring the structure matches and the collection name is sanitized.
        /// </summary>
        public static void AddTextSearchPlugin(Kernel kernel, YamlCollectionConfig collectionInfo)
        {
            if (kernel == null)
            {
                Console.WriteLine("Kernel is null.");
                return;
            }
            if (collectionInfo == null)
            {
                Console.WriteLine("Collection configuration is null.");
                return;
            }
            if (string.IsNullOrWhiteSpace(collectionInfo.Structure))
            {
                Console.WriteLine("Collection structure is null or empty.");
                return;
            }

            // Sanitize the collection name: remove any non-alphanumeric characters.
            string originalName = collectionInfo.Name ?? "";
            string collectionName = ConvertToValidConnectionName(originalName);
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                collectionName = "default";
            }

            // Check if the plugin already exists based on its name.
            if (kernel.Plugins.Any(plugin => plugin.Name.Equals(collectionName, System.StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"Plugin for collection '{collectionName}' already exists. Skipping addition.");
                return;
            }

            var vectorStore = kernel.Services.GetService<IVectorStore>();
            var textEmbeddingGeneration = kernel.Services.GetService<ITextEmbeddingGenerationService>();

            if (vectorStore == null)
            {
                Console.WriteLine("VectorStore service is not available.");
                return;
            }
            if (textEmbeddingGeneration == null)
            {
                Console.WriteLine("TextEmbeddingGenerationService is not available.");
                return;
            }

            var collection = vectorStore.GetCollection<Guid, T>(collectionName);
            if (collection == null)
            {
                Console.WriteLine($"Collection '{collectionName}' not found in the vector store.");
                return;
            }

            var textSearch = new VectorStoreTextSearch<T>(collection, textEmbeddingGeneration);
            var searchPlugin = textSearch.CreateWithGetTextSearchResults(collectionName);
            kernel.Plugins.Add(searchPlugin);
            Console.WriteLine($"Added text search plugin for collection '{collectionName}'.");
        }

        /// <summary>
        /// Converts a given string to a valid connection name by removing any non-alphanumeric characters.
        /// </summary>
        private static string ConvertToValidConnectionName(string name)
        {
            return ConnectNameRegex().Replace(name, "");
        }

        [GeneratedRegex(@"[^A-Za-z0-9]")]
        private static partial Regex ConnectNameRegex();


        public static async IAsyncEnumerable<VectorSearchResult<T>> GetRelatedDocuments(
             Kernel kernel,
             YamlCollectionConfig collectionInfo,
             string text,
             int top = 5) // default to 5 if not provided
        {
            if (kernel == null)
            {
                Console.WriteLine("Kernel is null.");
                yield break;
            }
            if (collectionInfo == null)
            {
                Console.WriteLine("Collection configuration is null.");
                yield break;
            }
            if (string.IsNullOrWhiteSpace(collectionInfo.Structure))
            {
                Console.WriteLine("Collection structure is null or empty.");
                yield break;
            }

            // Sanitize the collection name: remove any non-alphanumeric characters.
            string originalName = collectionInfo.Name ?? "";
            string collectionName = ConvertToValidConnectionName(originalName);
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                collectionName = "default";
            }

 
            var vectorStore = kernel.Services.GetService<IVectorStore>();
            var textEmbeddingGeneration = kernel.Services.GetService<ITextEmbeddingGenerationService>();

            if (vectorStore == null)
            {
                Console.WriteLine("VectorStore service is not available.");
                yield break;
            }
            if (textEmbeddingGeneration == null)
            {
                Console.WriteLine("TextEmbeddingGenerationService is not available.");
                yield break;
            }

            var collection = vectorStore.GetCollection<Guid, T>(collectionName);
            if (collection == null)
            {
                Console.WriteLine($"Collection '{collectionName}' not found in the vector store.");
                yield break;
            }

            // Generate a vector for your search text.
            ReadOnlyMemory<float> searchVector = await textEmbeddingGeneration.GenerateEmbeddingAsync(text);

            // Do the search, passing the top value to limit the number of results.
            var searchResult = await collection.VectorizedSearchAsync(searchVector, new() { Top = top });

            // Yield each record so that the caller can enumerate them.
            await foreach (var record in searchResult.Results)
            {
                yield return record;
            }
        }

        public static async IAsyncEnumerable<VectorSearchResult<T>> GetDocuments(
          Kernel kernel,
          YamlCollectionConfig collectionInfo,
          int top,
          int skip) // default to 5 if not provided
        {
            if (kernel == null)
            {
                Console.WriteLine("Kernel is null.");
                yield break;
            }
            if (collectionInfo == null)
            {
                Console.WriteLine("Collection configuration is null.");
                yield break;
            }
            if (string.IsNullOrWhiteSpace(collectionInfo.Structure))
            {
                Console.WriteLine("Collection structure is null or empty.");
                yield break;
            }

            // Sanitize the collection name: remove any non-alphanumeric characters.
            string originalName = collectionInfo.Name ?? "";
            string collectionName = ConvertToValidConnectionName(originalName);
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                collectionName = "default";
            }


            var vectorStore = kernel.Services.GetService<IVectorStore>();
            var textEmbeddingGeneration = kernel.Services.GetService<ITextEmbeddingGenerationService>();

            if (vectorStore == null)
            {
                Console.WriteLine("VectorStore service is not available.");
                yield break;
            }
           

            var collection = vectorStore.GetCollection<Guid, T>(collectionName);
            if (collection == null)
            {
                Console.WriteLine($"Collection '{collectionName}' not found in the vector store.");
                yield break;
            }

            T temp = new T();
            float[] dummyVectorArray = new float[temp.EmbeddingDimension];
            ReadOnlyMemory<float> queryVector = dummyVectorArray;
            

            // Do the search, passing the top value to limit the number of results.
            var searchResult = await collection.VectorizedSearchAsync(queryVector, new() { Top = Math.Max(top,1), Skip = Math.Max(skip,0) });

            // Yield each record so that the caller can enumerate them.
            await foreach (var record in searchResult.Results)
            {
                yield return record;
            }
        }

    }
}
