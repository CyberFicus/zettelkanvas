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

        /// <summary>
        /// Link text, including alias
        /// </summary>
        public string LinkText { get; private set; } 
        public string LinkComment { get; private set; }

        public LinkData(string linkText, string linkComment = "-") {
            LinkText = linkText;
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
