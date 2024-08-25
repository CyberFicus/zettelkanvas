using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private static string[] Symbols = ["$<$", "$>$", "$\\vdash$", "$\\odot$"];

        public string LinkText { get; private set; }
        public string LinkCommentary { get; private set; }

        public LinkData(string linkText, string linkCommentary = "-") {
            LinkText = linkText;
            LinkCommentary = linkCommentary;
        }

        public string Print(TypeSymbol type)
        {
            return $"{Symbols[(int)type]} [[{LinkText}]]: `{LinkCommentary}`\n";
        }
        public override string ToString()
        {
            return $"[[{LinkText}]]: `{LinkCommentary}`\n";
        }
    }
}
