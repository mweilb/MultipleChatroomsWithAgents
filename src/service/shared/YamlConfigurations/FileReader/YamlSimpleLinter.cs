using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using YamlConfigurations;

namespace YamlConfigurations.FileReader
{
    public class YamlLintResult
    {
        public string Message { get; set; } = string.Empty;
        public YamlLineInfo LineInfo { get; set; } = new YamlLineInfo();
    }

    public static class YamlSimpleLinter
    {
        public static List<YamlLintResult> Lint(string yamlText)
        {
            var errors = new List<YamlLintResult>();
            var lines = yamlText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var indentStack = new Stack<int>();
            var keySetStack = new Stack<HashSet<string>>();
            keySetStack.Push(new HashSet<string>());

            int openCurly = 0, openSquare = 0, openParen = 0;
            int? firstUnmatchedCurly = null, firstUnmatchedSquare = null, firstUnmatchedParen = null;

            bool inBlockScalar = false;
            var listIndentStack = new Stack<int?>();
            var listPrevLineStack = new Stack<int>();
            listIndentStack.Push(null);
            listPrevLineStack.Push(-1);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int lineNum = i + 1;

                // Detect start of block scalar (| or >)
                if (Regex.IsMatch(line, @"^\s*[\w\-]+:\s*[>|]"))
                    inBlockScalar = true;
                // Detect end of block scalar (next non-indented line or end of file)
                if (inBlockScalar && !string.IsNullOrWhiteSpace(line) && !Regex.IsMatch(line, @"^\s"))
                    inBlockScalar = false;

                // Skip empty or comment lines
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                // Trailing whitespace
                if (Regex.IsMatch(line, @"[ \t]+$"))
                    errors.Add(new YamlLintResult
                    {
                        Message = "Trailing whitespace detected.",
                        LineInfo = new YamlLineInfo { StartLine = lineNum, StartColumn = 1 }
                    });

                // Indentation: check for tabs
                if (line.StartsWith("\t"))
                    errors.Add(new YamlLintResult
                    {
                        Message = "Line starts with a tab character (YAML requires spaces).",
                        LineInfo = new YamlLineInfo { StartLine = lineNum, StartColumn = 1 }
                    });

                // Indentation: check for mixed tabs and spaces
                if (Regex.IsMatch(line, @"^( +\t|\t+ )"))
                    errors.Add(new YamlLintResult
                    {
                        Message = "Mixed spaces and tabs in indentation.",
                        LineInfo = new YamlLineInfo { StartLine = lineNum, StartColumn = 1 }
                    });

                // Unmatched quotes (skip check inside block scalars)
                if (!inBlockScalar)
                {
                    int singleQuotes = line.Count(c => c == '\'');
                    int doubleQuotes = line.Count(c => c == '"');
                    if (singleQuotes % 2 != 0)
                        errors.Add(new YamlLintResult
                        {
                            Message = "Unmatched single quote detected.",
                            LineInfo = new YamlLineInfo { StartLine = lineNum, StartColumn = 1 }
                        });
                    if (doubleQuotes % 2 != 0)
                        errors.Add(new YamlLintResult
                        {
                            Message = "Unmatched double quote detected.",
                            LineInfo = new YamlLineInfo { StartLine = lineNum, StartColumn = 1 }
                        });
                }

                // Bracket/brace/paren counting
                int openCurlyBefore = openCurly;
                int openSquareBefore = openSquare;
                int openParenBefore = openParen;

                openCurly += line.Count(c => c == '{') - line.Count(c => c == '}');
                openSquare += line.Count(c => c == '[') - line.Count(c => c == ']');
                openParen += line.Count(c => c == '(') - line.Count(c => c == ')');

                if (openCurly > 0 && openCurlyBefore == 0)
                    firstUnmatchedCurly ??= lineNum;
                if (openCurly < 0 && firstUnmatchedCurly == null)
                    firstUnmatchedCurly = lineNum;

                if (openSquare > 0 && openSquareBefore == 0)
                    firstUnmatchedSquare ??= lineNum;
                if (openSquare < 0 && firstUnmatchedSquare == null)
                    firstUnmatchedSquare = lineNum;

                if (openParen > 0 && openParenBefore == 0)
                    firstUnmatchedParen ??= lineNum;
                if (openParen < 0 && firstUnmatchedParen == null)
                    firstUnmatchedParen = lineNum;

                // Invalid line start
                if (Regex.IsMatch(line, @"^\s*[:\.,]"))
                    errors.Add(new YamlLintResult
                    {
                        Message = "Line starts with an invalid character for YAML.",
                        LineInfo = new YamlLineInfo { StartLine = lineNum, StartColumn = 1 }
                    });

                // Track indentation for mapping context
                int indent = line.TakeWhile(char.IsWhiteSpace).Count();
                if (indentStack.Count == 0 || indent > indentStack.Peek())
                {
                    indentStack.Push(indent);
                    keySetStack.Push(new HashSet<string>());
                    listIndentStack.Push(null);
                    listPrevLineStack.Push(-1);
                }
                else
                {
                    while (indentStack.Count > 0 && indent < indentStack.Peek())
                    {
                        indentStack.Pop();
                        keySetStack.Pop();
                        listIndentStack.Pop();
                        listPrevLineStack.Pop();
                    }
                }

                // List entry alignment check and key set reset (per parent context)
                if (Regex.IsMatch(line.TrimStart(), @"^-"))
                {
                    if (listIndentStack.Peek().HasValue && indent != listIndentStack.Peek().Value)
                    {
                        // Report error on previous list entry line
                        int prevLine = listPrevLineStack.Peek();
                        if (prevLine > 0)
                        {
                            errors.Add(new YamlLintResult
                            {
                                Message = "Misaligned list entry detected.",
                                LineInfo = new YamlLineInfo { StartLine = prevLine, StartColumn = 1 }
                            });
                        }
                    }
                    listIndentStack.Pop();
                    listIndentStack.Push(indent);

                    listPrevLineStack.Pop();
                    listPrevLineStack.Push(lineNum);

                    keySetStack.Pop();
                    keySetStack.Push(new HashSet<string>());
                }
                else
                {
                    var keyMatch = Regex.Match(line, @"^\s*([\w\-]+):");
                    if (keyMatch.Success)
                    {
                        var key = keyMatch.Groups[1].Value;
                        var currentKeySet = keySetStack.Peek();
                        if (currentKeySet.Contains(key))
                            errors.Add(new YamlLintResult
                            {
                                Message = $"Duplicate key '{key}' detected in mapping.",
                                LineInfo = new YamlLineInfo { StartLine = lineNum, StartColumn = 1 }
                            });
                        else
                            currentKeySet.Add(key);
                    }
                }
            }

            if (openCurly != 0)
                errors.Add(new YamlLintResult
                {
                    Message = "Unmatched curly braces detected.",
                    LineInfo = new YamlLineInfo { StartLine = firstUnmatchedCurly ?? lines.Length, StartColumn = 1 }
                });
            if (openSquare != 0)
                errors.Add(new YamlLintResult
                {
                    Message = "Unmatched square brackets detected.",
                    LineInfo = new YamlLineInfo { StartLine = firstUnmatchedSquare ?? lines.Length, StartColumn = 1 }
                });
            if (openParen != 0)
                errors.Add(new YamlLintResult
                {
                    Message = "Unmatched parentheses detected.",
                    LineInfo = new YamlLineInfo { StartLine = firstUnmatchedParen ?? lines.Length, StartColumn = 1 }
                });

            return errors;
        }
    }
}
