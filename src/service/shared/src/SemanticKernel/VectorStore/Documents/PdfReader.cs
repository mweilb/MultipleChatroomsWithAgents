 
using System.Text;
using UglyToad.PdfPig;
 

namespace MultiAgents.SemanticKernel.VectorStore.Documents
{
    internal static class PdfReader
    {

        public static (string[], string) ReadText(string pdfPath)
        {
            // Extract the full text from the PDF.
            var fullText = new StringBuilder();
            using (var document = PdfDocument.Open(pdfPath))
            {
                foreach (var page in document.GetPages())
                {
                    fullText.AppendLine(page.Text);
                }
            }

            // Split the text into paragraphs using newlines.
            string[] paragraphs = fullText.ToString()
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return (paragraphs, fullText.ToString());
        }
    }
}
