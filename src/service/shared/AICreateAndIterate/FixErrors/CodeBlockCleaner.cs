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
        /// <summary>
        /// Replaces all '{{' with '__LBRACE__' and all '}}' with '__RBRACE__' to protect from Handlebars parsing.
        /// </summary>
        public static string ProtectHandlebarsBraces(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Replace("{{", "__LBRACE__").Replace("}}", "__RBRACE__");
        }

        /// <summary>
        /// Restores all '__LBRACE__' to '{{' and '__RBRACE__' to '}}' after Handlebars processing.
        /// </summary>
        public static string RestoreHandlebarsBraces(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Replace("__LBRACE__", "{{").Replace("__RBRACE__", "}}");
        }
    }
}
