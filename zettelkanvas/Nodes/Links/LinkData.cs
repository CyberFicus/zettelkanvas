using System.Text.RegularExpressions;
using Zettelkanvas.Static;

namespace Zettelkanvas.Nodes.Links
{
    internal class LinkData
    {
        public string LinkedNoteName { get; private set; }
        public string LinkedNoteId { get; private set; }
        public string Alias { get; private set; }
        public string Comment { get; private set; }


        private static readonly string[] Symbols = ["$<$", "$>$", "$\\vdash$", "$\\odot$"];
        public static int GetType(string line)
        {
            for (int i = 0; i < Symbols.Length; i++)
            {
                if (line.Contains(Symbols[i]))
                    return i;
            }
            return -1;
        }

        public LinkData(string noteName, string noteId, string? alias = null, string comment = "-")
        {
            LinkedNoteName = noteName;
            LinkedNoteId = noteId;
            Alias = (alias is not null) ? alias : LinkedNoteId;
            Comment = comment;
        }
        public LinkData(Node toNode, string comment = "-")
        {
            LinkedNoteName = toNode.NoteName;
            LinkedNoteId = toNode.Id;
            Alias = LinkedNoteId;
            Comment = comment;
            bool noteHasLongName = toNode.Id.Length < toNode.NoteName.Length;
            if (noteHasLongName) 
                Alias = toNode.Id;           
        }

        private static LinkData? ValidateLinkMatch(Match linkMatch)
        {
            if (!linkMatch.Success) 
                return null;
            
            var noteNameMatch = Regexes.NoteName().Match(linkMatch.Value);
            if (!noteNameMatch.Success) 
                return null;
            var noteName = noteNameMatch.Value;

            var idMatch = Regexes.IdRegex().Match(noteName);
            if (!idMatch.Success) 
                return null;
            var noteId = idMatch.Value;

            var aliasMatch = Regexes.LinkAlias().Match(linkMatch.Value);
            string alias = noteId;
            bool hasAlias = (aliasMatch.Success);
            if (hasAlias)
                alias = aliasMatch.Value[1..^2];

            return new LinkData(noteName, noteId, alias);
        }
        public static List<LinkData> ProcessNoteSectionLine(string line)
        {
            List<LinkData> resList = [];
            foreach (Match linkMatch in Regexes.LinkRegex().Matches(line))
            {
                var link = ValidateLinkMatch(linkMatch);
                if (link is null) continue;

                if (link.Alias != link.LinkedNoteId)
                {
                    link.Comment = link.Alias;
                    link.Alias = link.LinkedNoteId;
                }

                resList.Add(link);
            }
            return resList;
        }
        public static LinkData? ProcessLinkSectionLine(string line)
        {
            var linkMatch = Regexes.LinkRegex().Match(line);
            var link = ValidateLinkMatch(linkMatch);
            if (link is null) return null;

            var commentMatch = Regexes.LinkComment().Match(line);
            string comment = (commentMatch.Success) ? commentMatch.Value[1..^1] : "-";
            link.Comment = comment;

            return link;
        }

        public override string ToString()
        {
            var alias = (Alias is not null) ? $"|{Alias}" : "";
            return $"[[{LinkedNoteName}{alias}]]: `{Comment}`";
        }
        public string Print(LinkType type)
        {
            return $"{Symbols[(int)type]} {this}";
        }
    }
}
