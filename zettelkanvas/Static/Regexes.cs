using System.Text.RegularExpressions;

namespace Zettelkanvas.Static
{
    internal static partial class Regexes
    {
        // Searhes for zettelkasten id in note's name
        [GeneratedRegex(@"^\d+[a-zA-Z\d]*([\s]|$)")]
        public static partial Regex IdInFileName();

        // Splits id string into elements
        [GeneratedRegex(@"\d+[a-zA-Z]*")]
        public static partial Regex IdElement();
        
        // Splits id element string into parts
        [GeneratedRegex(@"\d+|[a-zA-Z]+")]
        public static partial Regex IdElementPart();

        // Searches for any obsidian link (possibly aliased) in note's text
        [GeneratedRegex(@"\[\[[^\]]+\]\]")]
        public static partial Regex ObsidianLink();

        // Searches for note name with zettelkasten ID in link from note's text
        [GeneratedRegex(@"[a-zA-Z\d]+[^\]^\|]*")]
        public static partial Regex NoteName();

        // Searches for link comment in links section of note's text
        [GeneratedRegex(@"`.+`")]
        public static partial Regex LinkComment();

        // Searches for link alias in link from note's text
        [GeneratedRegex(@"\|[^\]]+\]\]")]
        public static partial Regex LinkAlias();
    }
}
