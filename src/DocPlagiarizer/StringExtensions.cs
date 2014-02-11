using System.Text.RegularExpressions;

namespace DocPlagiarizer
{
    public static class StringExtensions
    {
        public static string WithoutIndentation(this string @this)
        {
            var result = Regex.Replace(@this, @"^[\t +]", "");
            return Regex.Replace(result, @"(\r\n|\n)[\t ]+", "$1");
        }
    }
}