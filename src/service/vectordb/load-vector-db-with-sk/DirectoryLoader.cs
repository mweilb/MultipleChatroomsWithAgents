using api.src.SemanticKernel.VectorStore;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using MultiAgents.SemanticKernel.VectorStore;

namespace LoadVectorDbWithSk
{
    /// <summary>
    /// Recursively loads files from a directory into the vector store.
    /// This generic version uses type parameter T (which inherits from TextParagraph) to pass
    /// the desired paragraph type to the VectorStoreHelper.
    /// </summary>
    /// <typeparam name="T">
    /// The type of TextParagraph to use. T must inherit from TextParagraph and have a parameterless constructor.
    /// </typeparam>
    public class DirectoryLoader<T> where T : TextParagraph, new()
    {
        /// <summary>
        /// Loads files from the specified directory (with tilde expansion) using the provided file filters,
        /// and processes them via the Semantic Kernel.
        /// </summary>
        /// <param name="directoryPath">The directory path to load files from (supports tilde paths, e.g. "~/folder").</param>
        /// <param name="kernel">The Semantic Kernel instance used for processing.</param>
        /// <param name="collectionName">The collection name in your vector store.</param>
        /// <param name="maxChunkLength">The maximum chunk length for file processing.</param>
        /// <param name="fileFilters">An array of file filters (e.g. "*.txt", "*.pdf").</param>
        /// <returns>An asynchronous task representing the file loading operation.</returns>
        public static async Task LoadFilesFromDirectory(
            string directoryPath,
            string[] fileFilters,
            string[] directoriesToIgnore,
            string[] questions,
            Kernel kernel,
            string collectionName
            )
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory does not exist: {directoryPath}");
                return;
            }
            
            // If no filters are provided, default to "*.*"
            if (fileFilters == null || fileFilters.Length == 0)
            {
                fileFilters = new string[] { "*.*" };
            }

            // Recursively get all files from the directory and its subdirectories.
            var files = fileFilters
             .SelectMany(filter => Directory.GetFiles(directoryPath, filter, SearchOption.AllDirectories))
             .Distinct()
             // Filter out files that are in any of the directories to ignore.
             .Where(file =>
             {
                 string fileDir = Path.GetDirectoryName(file) ?? string.Empty;
                 // Check if any ignored directory is a substring of the file directory (case-insensitive)
                 return !directoriesToIgnore.Any(ignore =>
                     fileDir.IndexOf(ignore, StringComparison.OrdinalIgnoreCase) >= 0);
             })
            .ToArray();

            var vectorStore = kernel.Services.GetService<IVectorStore>();
            var collection = vectorStore?.GetCollection<Guid,T>(collectionName);
            if (collection != null)
            {
                // Ensure that the collection exists; if it does not, create it.
                await collection.CreateCollectionIfNotExistsAsync();
            }

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file).ToLowerInvariant();

                try
                {
                    var oldEnteries = await SearchByDocumentUriAsync(collection, file);
                    await BatchDeleteRecordsAsync(collection, oldEnteries);

                    switch (extension)
                    {
                        case ".pdf":
                            Console.WriteLine($"Processing PDF: {file}");
                            await VectorStoreHelper<T>.SavePdfToVectorStore(file, kernel, collectionName, questions);
                            break;

                        case ".docx":
                        case ".doc":
                            // Create a simple file URL.
                            string fileUrl = "file://" + file;
                            Console.WriteLine($"Processing Word Document: {file}");
                            await VectorStoreHelper<T>.SaveWordToVectorStore(fileUrl, file, kernel, collectionName, questions);
                            break;

                        case ".txt":
                            Console.WriteLine($"Processing Text File: {file}");
                            await VectorStoreHelper<T>.SaveTextToVectorStore(file, kernel, collectionName, questions);
                            break;

                        default:
                            // For any other extension, check if the file is binary.
                            if (!IsBinaryFile(file))
                            {
                                Console.WriteLine($"Processing as text: {file}");
                                await VectorStoreHelper<T>.SaveTextToVectorStore(file, kernel, collectionName, questions);
                            }
                            else
                            {
                                Console.WriteLine($"Skipping unsupported binary file: {file}");
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Checks if a file is binary by reading a sample of its bytes.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns>True if the file is binary; otherwise, false.</returns>
        private static bool IsBinaryFile(string filePath)
        {
            const int sampleSize = 8000;
            byte[] buffer = new byte[sampleSize];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                int bytesRead = fs.Read(buffer, 0, sampleSize);
                for (int i = 0; i < bytesRead; i++)
                {
                    // If a null byte is detected, consider the file binary.
                    if (buffer[i] == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Searches the provided collection for all TextParagraphEmbeddingOf1536 records that have the specified DocumentUri.
        /// </summary>
        /// <param name="collection">
        /// The vector store collection to search. If null, the function returns an empty list.
        /// </param>
        /// <param name="targetDocumentUri">The DocumentUri to filter by.</param>
        /// <returns>A list of matching TextParagraphEmbeddingOf1536 records or an empty list if the collection is null.</returns>
        public static async Task<List<T>> SearchByDocumentUriAsync(
            IVectorStoreRecordCollection<Guid, T>? collection,
            string targetDocumentUri)
        {
            // If the collection is null, return an empty list.
            if (collection == null)
            {
                return new List<T>();
            }

            // Setup vector search options with a filter on the DocumentUri.
            var searchOptions = new VectorSearchOptions<T>
            {
                Filter = tp => tp.DocumentUri == targetDocumentUri,
            };

            // Provide a dummy vector (an array of zeros) with a length that matches the embedding dimension  
            T temp = new T();
            float[] dummyVectorArray = new float[temp.EmbeddingDimension];
            ReadOnlyMemory<float> queryVector = dummyVectorArray;

            // Execute the vectorized search with the provided filter.
            var searchResult = await collection.VectorizedSearchAsync(queryVector, searchOptions);

            // Collect and return the results.
            var resultList = new List<T>();
            await foreach (var result in searchResult.Results)
            {
                resultList.Add(result.Record);
            }

            return resultList;
        }

        /// <summary>
        /// Deletes a batch of records from the provided collection.
        /// </summary>
        /// <param name="collection">
        /// The vector store collection to delete from. If null, the function returns immediately.
        /// </param>
        /// <param name="records">
        /// The list of records to delete. If null or empty, no action is taken.
        /// </param>
        public static async Task BatchDeleteRecordsAsync(
            IVectorStoreRecordCollection<Guid, T>? collection,
            List<T>? records)
        {
            if (collection == null || records == null || records.Count == 0)
            {
                return;
            }

            // Create an array of GUIDs from the records
            Guid[] keysToDelete = records.Select(record => record.Key).ToArray();
            await collection.DeleteBatchAsync(keysToDelete);
        }
    }
}
