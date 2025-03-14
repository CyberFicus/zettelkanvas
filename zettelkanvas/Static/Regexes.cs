﻿using System.Text.RegularExpressions;

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

        // Searches for hex color starting with # character
        [GeneratedRegex(@"#[\da-f]{6}")]
        public static partial Regex HexColor();

        // Searches for a formatted value in config
        [GeneratedRegex(@"^\s*[\w\d]+\b\s*:\s*[\d]+")]
        public static partial Regex ConfigValue();

        // Searches for a formatted color constant in config
        [GeneratedRegex(@"^\s*\@[\w\d]+\b\s*:\s*(#[\da-f]{6}|\d|\-)+")]
        public static partial Regex ConfigColor();

        // Matches a property written in one line
        [GeneratedRegex(@"^\s*(([\w]+)(\s+[\w]+)*):\s*(([\w.:,;!?\(\)`""`/|\\'&%№*-–—+]+)(\s+[\w.:,;!?\(\)`""`/|\\'&%№*-–—+]+)*)")]
        public static partial Regex PropertyOneLine();

        // Matches a beginning of multiline property
        [GeneratedRegex(@"^\s*(([\w]+)(\s+[\w]+)*):\W*$")]
        public static partial Regex PropertyMultiLineStart();

        // Matches a part of multiline property
        [GeneratedRegex(@"^\s*-\s*(([\w.:,;!?\(\)`""`/|\\'&%№*-–—+]+)(\s+[\w.:,;!?\(\)`""`/|\\'&%№*-–—+]+)*)")]
        public static partial Regex PropertyMultiLinePart();
    }
}
