#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Import necessary namespaces for handling vector data and text embeddings.
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace MultiAgents.SemanticKernel.VectorStore.Documents
{
    /// <summary>
    /// DataUploader is responsible for generating text embeddings and uploading text paragraphs 
    /// to a vector store collection.
    /// </summary>
    /// <remarks>
    /// This class is marked as internal and is intended for evaluation purposes only.
    /// </remarks>
    internal class DataUploader(IVectorStore vectorStore, ITextEmbeddingGenerationService textEmbeddingGenerationService)
    {
         
        /// <summary>
        /// Generates an embedding for each text paragraph of type T and uploads it to the specified collection.
        /// </summary>
        /// <typeparam name="T">A type that derives from TextParagraph and has a parameterless constructor.</typeparam>
        /// <param name="collectionName">The name of the collection to upload the text paragraphs to.</param>
        /// <param name="textParagraphs">The text paragraphs to process and upload.</param>
        /// <returns>An asynchronous task representing the upload operation.</returns>
        public async Task GenerateEmbeddingsAndUpload<T>(string collectionName, string uri,string paragraphId, string question, string? answer, string fullText)
            where T : TextParagraph, new()
        {
            // Retrieve the collection from the vector store using the specified collection name.
            var collection = vectorStore.GetCollection<Guid, T>(collectionName);

            // Ensure that the collection exists; if it does not, create it.
            await collection.CreateCollectionIfNotExistsAsync();

            var entry = new T
            {
                Key = Guid.NewGuid(),
                ParagraphId = paragraphId,
                DocumentUri = uri,
                Text = fullText,
                Question = question,
                Answer = answer??"full text",
                // Generate the text embedding for the paragraph using the provided text embedding generation service.
                TextEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(answer?? fullText)
            };
            try
            {
                await collection.UpsertAsync(entry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing paragraph {entry.ParagraphId}: {ex.Message}");
            }
        }
    }
}
