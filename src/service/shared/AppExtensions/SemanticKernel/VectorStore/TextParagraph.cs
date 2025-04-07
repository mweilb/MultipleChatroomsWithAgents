using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable SKEXP0001
namespace AppExtensions.SemanticKernel.VectorStore
{
    

    public class TextParagraph
    {
        /// <summary>A unique key for the text paragraph.</summary>
        [VectorStoreRecordKey]
        [TextSearchResultName]
        public required Guid Key { get; init; } = Guid.Empty;

        /// <summary>A uri that points at the original location of the document containing the text.</summary>
        [VectorStoreRecordData(IsFilterable = true)]
        [TextSearchResultLink]
        public required string DocumentUri { get; init; } = string.Empty;

        /// <summary>The id of the paragraph from the document containing the text.</summary>
        [VectorStoreRecordData]
        public required string ParagraphId { get; init; } = string.Empty;

        /// <summary>The text of the paragraph.</summary>
        [VectorStoreRecordData(IsFilterable = true)]
        [TextSearchResultValue]
        public required string Text { get; init; } = string.Empty;

        /// <summary>The text of the paragraph.</summary>
        [VectorStoreRecordData(IsFilterable = true)]
        public required string Question { get; init; } = string.Empty;

        /// <summary>The text of the paragraph.</summary>
        [VectorStoreRecordData(IsFilterable = false)]
        public required string Answer { get; init; } = string.Empty;


        // Provide a default implementation for TextEmbedding.
        public virtual ReadOnlyMemory<float> TextEmbedding { get; set; } = ReadOnlyMemory<float>.Empty;

        // Derived classes must specify the expected vector dimension.
        [VectorStoreRecordData(IsFilterable = true)]
        public int EmbeddingDimension { get; set; } = 0;

    }

    public class TextParagraphEmbeddingOf1536 : TextParagraph
    {
        [SetsRequiredMembers]
        public TextParagraphEmbeddingOf1536()
        {
            EmbeddingDimension = 1536;
            Key = Guid.Empty;
            DocumentUri = string.Empty;
            ParagraphId = string.Empty;
            Text = string.Empty;
            Question = string.Empty;
            TextEmbedding = ReadOnlyMemory<float>.Empty;
            Answer = string.Empty;
        }
        /// <summary>The embedding generated from the Text.</summary>
        [VectorStoreRecordVector(1536)]
        public override ReadOnlyMemory<float> TextEmbedding { get; set; } = ReadOnlyMemory<float>.Empty;
    }

    public class TextParagraphEmbeddingOf3584 : TextParagraph
    {
        [SetsRequiredMembers]
        public TextParagraphEmbeddingOf3584()
        {
            EmbeddingDimension = 3584;
            Key = Guid.Empty;
            DocumentUri = string.Empty;
            ParagraphId = string.Empty;
            Text = string.Empty;
            Question = string.Empty;
            TextEmbedding = ReadOnlyMemory<float>.Empty;
            Answer = string.Empty;
        }
        /// <summary>The embedding generated from the Text.</summary>
        [VectorStoreRecordVector(3584)]
        public override ReadOnlyMemory<float> TextEmbedding { get; set; } = ReadOnlyMemory<float>.Empty;
    }
}
