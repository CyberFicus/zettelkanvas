using System.Text.RegularExpressions;

namespace zettelkanvas
{
    internal static partial class Regexes
    {
        // Searhes for zettelkasten id in note's name
        [GeneratedRegex(@"\d+[a-zA-Z\d]*\b")]
        public static partial Regex IdRegex();

        // Splits id string into elements
        [GeneratedRegex(@"\d+[a-zA-Z]*")]
        public static partial Regex IdElement();
        
        // Splits id element string into parts
        [GeneratedRegex(@"\d+|[a-zA-Z]+")]
        public static partial Regex IdElementPart();

        // Searches for obsidian link (possibly aliased) in note's text
        [GeneratedRegex(@"\[\[\s*([a-zA-Z\d])+[\w\s]*(\||[\w\s])*\]\]")]
        public static partial Regex LinkRegex();

        // Searches for note name in link from note's text
        [GeneratedRegex(@"([a-zA-Z\d])+[\w\s]*")]
        public static partial Regex NoteName();

        // Searches for link alias in link from note's text
        [GeneratedRegex(@"`.+`")]
        public static partial Regex LinkComment();

        // Searches for link comment in links section of note's text
        [GeneratedRegex(@"\|[^\]]+\]\]")]
        public static partial Regex LinkAlias();
    }
}
