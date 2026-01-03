using System.Text.RegularExpressions;

namespace DatabaseManager.Core
{
    public class ScriptHelper
    {
        private static Regex noEOLchar = new Regex("[^\r\n]");

        /*
         * This refers to: https://renenyffenegger.ch/notes/development/databases/SQL/statement/comment/remove/C-Sharp
         */
        public static string RemoveComments(string script, bool removeLiterals = false, bool preservePositions = false)
        {
            var lineComment = @"--(.*?)\r?\n";
            var lineCommentNoEOL = @"--(.*?)$"; // A line comment that has no end of line.

            //
            // Literals, bracketed identifiers and quotedIdentifiers ("object") all have the same structure:
            //     - Start character,
            //     - a number of characters (including consecutive pairs of closing
            //       characters which are counted as part of the «thing), and
            //     - the closing character
            //
            var literals = @"('(('')|[^'])*')";          // 'John', 'O''malley''s', etc
            var bracketedIdentifiers = @"\[((\]\])|[^\]])* \]";      // [object], [ % object]] ], etc
            var quotedIdentifiers = @"(\""((\""\"")|[^""])*\"")"; // "object", "object[]", etc - when QUOTED_IDENTIFIER is set to ON, they are identifiers, else they are literals

            //
            // The original code was for C#, but Microsoft SQL allows a nested block comments
            //     https://msdn.microsoft.com/en-us/library/ms178623.aspx
            // so we should use balancing groups
            //   -> http://weblogs.asp.net/whaggard/377025
            //
            // var blockComments = @"/\*(.*?)\*/";
            //
            var nestedBlockComments = @"/\*
                                    (?>
                                    /\*  (?<LEVEL>)     # On opening push level
                                    |
                                    \*/ (?<-LEVEL>)     # On closing pop level
                                    |
                                    (?! /\* | \*/ ) .   # Match any char unless the opening and closing strings
                                    )+                  # /* or */ in the lookahead string
                                    (?(LEVEL)(?!))      # If level exists then fail
                                    \*/";

            string noComments = Regex.Replace(script,
                nestedBlockComments + "|" +
                lineComment + "|" +
                lineCommentNoEOL + "|" +
                literals + "|" +
                bracketedIdentifiers + "|" +
                quotedIdentifiers,

                txt => {

                    if (txt.Value.StartsWith("/*"))
                    {

                        if (preservePositions)
                        {
                            //  preserve positions and keep line-breaks
                            return noEOLchar.Replace(txt.Value, " ");
                        }                            

                        return "";

                    }

                    if (txt.Value.StartsWith("--"))
                    {
                        if (preservePositions)
                        {
                            //  preserve positions and keep line-breaks
                            return noEOLchar.Replace(txt.Value, " ");
                        }                           

                        //  preserve only line-breaks
                        return noEOLchar.Replace(txt.Value, "");
                    }

                    if (txt.Value.StartsWith("[") || txt.Value.StartsWith("\""))
                    {
                        //  Don't ever remove object identifiers
                        return txt.Value;
                    }

                    if (!removeLiterals)
                    {
                        //  Keep literal strings
                        return txt.Value;
                    }

                    if (preservePositions)
                    {
                        //
                        //  Remove literals, but preserve positions and line-breaks
                        //
                        var literalWithLineBreaks = noEOLchar.Replace(txt.Value, " ");

                        return "'" + literalWithLineBreaks.Substring(1, literalWithLineBreaks.Length - 2) + "'";
                    }

                    return "''";
                },

                RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace
            );

            return noComments;
        }
    }
}
