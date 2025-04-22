namespace AICreateAndIterate.FixErrors
{
    public static class PromptJsonCleaner
    {
        /// <summary>
        /// Removes code block markers (```json, ```) from a string for easier JSON parsing.
        /// </summary>
        public static string CleanJsonBlock(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var cleaned = input.Trim();
            if (cleaned.StartsWith("```json"))
            {
                cleaned = cleaned.Substring(7).TrimStart();
            }
            if (cleaned.StartsWith("```"))
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
