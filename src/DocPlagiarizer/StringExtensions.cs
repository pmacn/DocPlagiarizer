using System.Text.RegularExpressions;

namespace DocPlagiarizer
{
    public static class StringExtensions
    {
        public static string WithoutIndentation(this string @this)
        {
            return Regex.Replace(@this, @"^[\t ]+", "", RegexOptions.Multiline);
        }

        public static string WithIndentation(this string @this, string indentation)
        {
            return Regex.Replace(@this, "([\n])[\t ]*", "$1" + indentation);
        }
    }
}