namespace zettelkanvas
{
    internal class LinkData
    {
        public enum TypeSymbol
        {
            PrevLink,
            NextLink,
            BranchLink,
            OuterLink
        }
        private static readonly string[] Symbols = ["$<$", "$>$", "$\\vdash$", "$\\odot$"];
        public static int GetType(string line)
        {
            for(int i = 0; i < Symbols.Length; i++)
            {
                if (line.Contains(Symbols[i]))
                    return i;
            }
            return -1;
        }


        /// <summary>
        /// Link text, including alias
        /// </summary>
        public string LinkText { get; private set; } 
        public string LinkComment { get; private set; }

        public LinkData(string linkText, string linkComment = "-") {
            LinkText = linkText;
            LinkComment = linkComment;
        }
        public LinkData(Node toNode, string linkComment = "-")
        {
            bool longName = (toNode.Id.Length < toNode.NoteName.Length);
            LinkText = (longName) ? $"{toNode.NoteName}|{toNode.Id}" : toNode.Id;
            LinkComment = linkComment;
        }

        public string Print(TypeSymbol type)
        {
            return $"{Symbols[(int)type]} [[{LinkText}]]: `{LinkComment}`";
        }
        public override string ToString()
        {
            return $"[[{LinkText}]]: `{LinkComment}`\n";
        }
    }
}
