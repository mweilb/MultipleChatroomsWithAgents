using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using api.src.SemanticKernel.VectorStore;
 
namespace MultiAgents.SemanticKernel.VectorStore.Documents
{
    /// <summary>
    /// Reads a text document, splits its content into chunks of up to maxChunkLength characters,
    /// and yields each chunk as an instance of T.
    /// </summary>
    /// <typeparam name="T">
    /// The type of TextParagraph to return. T must inherit from TextParagraph and have a parameterless constructor.
    /// </typeparam>
    internal static class TextFileReader
    { 
        public static (string[], string) ReadText(string filePath)
        {
            // Read the entire content from the text file.
            string fullText = File.ReadAllText(filePath);

            // Split the text into paragraphs using newlines.
            string[] paragraphs = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return (paragraphs, fullText);
        }
    }
}
