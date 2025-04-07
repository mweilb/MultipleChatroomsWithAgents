using System.Text;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;

namespace AppExtensions.SemanticKernel.VectorStore.Documents
{
    internal static class DocumentReader
    {


        /// <summary>
        /// Reads the text from a Word document stream and returns an array of paragraphs along with the full text.
        /// </summary>
        /// <param name="documentContents">A stream containing the Word document.</param>
        /// <param name="documentUri">A URI identifying the document.</param>
        /// <returns>A tuple where the first element is an array of paragraph strings, and the second element is the full text.</returns>
        public static (string[] Paragraphs, string FullText) ReadText(Stream documentContents, string documentUri)
        {
            using WordprocessingDocument wordDoc = WordprocessingDocument.Open(documentContents, false);
            if (wordDoc.MainDocumentPart == null)
            {
                return (Array.Empty<string>(), string.Empty);
            }

            // Load the document into an XmlDocument.
            XmlDocument xmlDoc = new XmlDocument();
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            nsManager.AddNamespace("w14", "http://schemas.microsoft.com/office/word/2010/wordml");
            xmlDoc.Load(wordDoc.MainDocumentPart.GetStream());

            // Select all paragraph nodes.
            XmlNodeList? paragraphNodes = xmlDoc.SelectNodes("//w:p", nsManager);
            if (paragraphNodes == null)
            {
                return (Array.Empty<string>(), string.Empty);
            }

            List<string> paragraphs = new List<string>();
            StringBuilder fullTextBuilder = new StringBuilder();

            // Iterate over each paragraph.
            foreach (XmlNode paragraph in paragraphNodes)
            {
                XmlNodeList? texts = paragraph.SelectNodes(".//w:t", nsManager);
                if (texts == null)
                {
                    continue;
                }

                StringBuilder paragraphBuilder = new StringBuilder();
                foreach (XmlNode text in texts)
                {
                    if (!string.IsNullOrWhiteSpace(text.InnerText))
                    {
                        paragraphBuilder.Append(text.InnerText);
                    }
                }
                string combinedText = paragraphBuilder.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(combinedText))
                {
                    paragraphs.Add(combinedText);
                    fullTextBuilder.AppendLine(combinedText);
                }
            }

            return (paragraphs.ToArray(), fullTextBuilder.ToString());
        }


    }
}