namespace AICreateAndIterate.FixErrors
{
    public static class CodeBlockCleaner
    {
        /// <summary>
        /// Removes code block markers (```json, ```yaml, ```, etc.) from a string for easier parsing.
        /// </summary>
        public static string CleanCodeBlock(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var cleaned = input.Trim();
            if (cleaned.StartsWith("```json"))
            {
                cleaned = cleaned.Substring(7).TrimStart();
            }
            else if (cleaned.StartsWith("```yaml"))
            {
                cleaned = cleaned.Substring(7).TrimStart();
            }
            else if (cleaned.StartsWith("```"))
            {
                cleaned = cleaned.Substring(3).TrimStart();
            }
            if (cleaned.EndsWith("```"))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - 3).TrimEnd();
            }
            return cleaned;
        }
    }
}
