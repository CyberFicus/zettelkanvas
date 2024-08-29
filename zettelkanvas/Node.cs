using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace zettelkanvas
{
    internal class Node : IComparable<Node>
    {
        [JsonPropertyName("id")]
        public string Id { get; private set; }
        [JsonPropertyName("file")]
        public string FileProperty { get; private set; }
        [JsonPropertyName("type")]
        public string Type { get { return "file"; } }
        [JsonPropertyName("x")]
        public int OutputX { get { return X * 600; } }
        [JsonPropertyName("y")]
        public int OutputY { get { return Y * 600; } }
        [JsonPropertyName("width")]
        public int Width { get { return 400; } }
        [JsonPropertyName("height")]
        public int Height { get { return 400; } }

        [JsonIgnore]
        public static Parameters? Parameters { get; set; } = null;
        [JsonIgnore]
        public string NotePath { get; private set; }
        [JsonIgnore]
        public IdData IdData { get; set; }
        [JsonIgnore]
        public string NoteName { get; private set; }
        [JsonIgnore]
        public bool IsRoot { get; private set; } = false;
        [JsonIgnore]
        public int X { get; set; } = 0;
        [JsonIgnore]
        public int Y { get; set; } = 0;

        [JsonIgnore]
        public Node? Parent { get; private set; }
        [JsonIgnore]
        public Node? Next { get; private set; }
        [JsonIgnore]
        public List<Node> Branches { get;} = new List<Node>();

        public Node(string notePath, string id, string noteName)
        {
            NotePath = notePath;
            Id = id;
            IdData = new(id);
            NoteName = noteName;
#pragma warning disable CS8602
            FileProperty = Parameters.FilePropertyBase + NoteName + ".md";
#pragma warning restore CS8602
            if (int.TryParse(id, out _) && NoteName.Contains(Parameters.RootNodeIndicator))
                IsRoot = true;
        }
 
        public void RemoveNext()
        {
            if (Next is null) return;
            Next.Parent = null;
            Next = null;
        }
        public void SetNext(Node node)
        {
            RemoveNext();
            node.Parent = this;
            Next = node;
        }
        public void AddBranch(Node node)
        {
            Branches.Add(node);
            node.Parent = this;
        }

        public void Move(int xDif, int yDif)
        {
            X += xDif;
            Y += yDif;
        }
        public void MoveFromNode(Node anchor, int xDif, int yDif)
        {
            X = anchor.X;
            Y = anchor.Y;
            Move(xDif, yDif);
        }
        public void Arrange(out int length, out int height)
        {
            length = 1; height = 1;
            int lengthBuf = 0, heightBuf = 0;
            for (int i = 0; i < Branches.Count; i++)
            {
                Branches[i].MoveFromNode(this, 1, height);
                Branches[i].Arrange(out lengthBuf, out heightBuf);
                height += heightBuf;
                length = int.Max(length, lengthBuf);
            }

            if (Next is not null)
            {
                Next.MoveFromNode(this, length, 0);
                Next.Arrange(out lengthBuf, out heightBuf);
                length += lengthBuf; height = int.Max(height, heightBuf);
            }
        }

        private struct LinksInfo
        {
            public LinkData? previousNodeLink = null;
            public LinkData? nextNodeLink = null;
            public Dictionary<string, LinkData> unclassidiedLinks = [];
            public Dictionary<string, LinkData> branchLinks = [];
            public Dictionary<string, LinkData> outerLinks = [];

            public LinksInfo() { }
        }
        private void SplitText(out List<string> originalText, out List<string> noteSection, out List<string> linkSection)
        {
            originalText = new(File.ReadAllLines(NotePath));
            noteSection = [];
            linkSection = [];         

            for (int i = 0; i < originalText.Count; i++)
            {
                if (originalText[i].Contains("%%ZK:links%%"))
                {
                    if (i < originalText.Count - 1)
                        linkSection = originalText[(i + 1)..];
                    break;
                }
                noteSection.Add(originalText[i]);
            }
        }
        private LinksInfo ProcessLinks(List<string> noteSection, List<string> linkSection)
        {
            LinksInfo links = new();

            bool ValidateLinkMatch(string linkText, out string noteName, out string noteId)
            {
                noteName = "";
                noteId = "";

                var noteNameMatch = Regexes.NoteName().Match(linkText);
                if (!noteNameMatch.Success) return false;
                noteName = noteNameMatch.Value;

                var idMatch = Regexes.IdRegex().Match(noteName);
                if (!idMatch.Success) return false;
                noteId = idMatch.Value;
                if (links.unclassidiedLinks.ContainsKey(noteId)) return false;

                return true;
            }
            void AssignLink(string line, string id, LinkData link)
            {
                int linkType = LinkData.GetType(line);
                switch (linkType)
                {
                    case (int)LinkData.TypeSymbol.PrevLink:
                        if (links.previousNodeLink == null)
                            links.previousNodeLink = link;
                        break;
                    case (int)LinkData.TypeSymbol.NextLink:
                        if (links.nextNodeLink == null)
                            links.nextNodeLink = link;
                        break;
                    case (int)LinkData.TypeSymbol.BranchLink:
                        links.branchLinks.TryAdd(id, link);
                        break;
                    case (int)LinkData.TypeSymbol.OuterLink:
                        links.outerLinks.TryAdd(id, link);
                        break;
                }
                if (links.unclassidiedLinks.ContainsKey(id))
                    links.unclassidiedLinks.Remove(id);
            }
            void ProcessNoteSectionLine(string line)
            {
                foreach (Match linkMatch in Regexes.LinkRegex().Matches(line))
                {
                    string parsedLinkText = linkMatch.Value[2..^2];
                    bool drop = !ValidateLinkMatch(parsedLinkText, out var linkedNoteName, out var linkedNoteId);
                    if (drop) continue;

                    var newLinkText = linkedNoteName;
                    bool linkedNoteHasLongName = (parsedLinkText.Length > linkedNoteId.Length);
                    if (linkedNoteHasLongName)
                        newLinkText += $"|{linkedNoteId}";

                    var aliasMatch = Regexes.LinkAlias().Match(linkMatch.Value);
                    var autoLinkComment = "-";
                    if (aliasMatch.Success)
                        autoLinkComment = aliasMatch.Value[1..^2];

                    LinkData link = new(newLinkText, autoLinkComment);
                    links.unclassidiedLinks.Add(linkedNoteId, link);
                }
            }
            void ProcessLinkSectionLine(string line)
            {
                Match linkMatch = Regexes.LinkRegex().Match(line);
                if (!linkMatch.Success) return;

                string parsedLinkText = linkMatch.Value[2..^2]; ;
                bool drop = !ValidateLinkMatch(parsedLinkText, out var linkedNoteName, out var linkedNoteId);
                if (drop) return;

                var newLinkText = linkedNoteName;
                bool linkHasAlias = parsedLinkText.Contains('|');
                if (linkHasAlias)
                    newLinkText = parsedLinkText;
                bool linkedNoteHasLongName = (parsedLinkText.Length > linkedNoteId.Length);
                if (!linkHasAlias && linkedNoteHasLongName) newLinkText += $"|{linkedNoteId}";

                var matchComment = Regexes.LinkComment().Match(line);
                var linkComment = (matchComment.Success) ? matchComment.Value[1..^1] : "-";

                LinkData link = new(newLinkText, linkComment);
                AssignLink(line, linkedNoteId, link);
            }

            foreach (string line in noteSection)
                ProcessNoteSectionLine(line);
            foreach (string line in linkSection)
                ProcessLinkSectionLine(line);

            return links;
        }
        private List<string> FormLinkSectionAndEdges(LinksInfo links, Dictionary<string, Node> idToNode, out List<Edge> edges)
        {
            List<string> newLinkSection = [];
            edges = new();

            LinkData GetLinkFromNodeOrUnclassified(Node node)
            {
                if (links.unclassidiedLinks.TryGetValue(node.Id, out var link))
                {
                    links.unclassidiedLinks.Remove(node.Id);
                    return link;
                }
                return new LinkData(node);
            }

            if (Parent is not null)
            {
                links.previousNodeLink = (links.previousNodeLink is null) ? GetLinkFromNodeOrUnclassified(Parent) : links.previousNodeLink;
                newLinkSection.Add(links.previousNodeLink.Print(LinkData.TypeSymbol.PrevLink));
                edges.Add(Edge.TreeLink(Parent.Id, Id));
            }

            if (Next is not null)
            {
                links.nextNodeLink = (links.nextNodeLink is null) ? GetLinkFromNodeOrUnclassified(Next) : links.nextNodeLink;
                newLinkSection.Add(links.nextNodeLink.Print(LinkData.TypeSymbol.NextLink));
            }

            foreach (Node branch in Branches)
            {
                LinkData? branchLink;
                bool set = links.branchLinks.TryGetValue(branch.Id, out branchLink);
                branchLink = (set) ? branchLink : GetLinkFromNodeOrUnclassified(branch);

                Debug.Assert(branchLink is not null);
                if (branchLink is null)
                {
                    Console.Error.WriteLine($"Error: unable to set link from {Id} to {branch.Id}");
                    continue;
                }
                newLinkSection.Add(branchLink.Print(LinkData.TypeSymbol.BranchLink));
            }

            foreach (var pair in links.unclassidiedLinks)
                links.outerLinks.TryAdd(pair.Key, pair.Value);
            List<Node> outerLinkedNodes = [];
            foreach (var pair in links.outerLinks)
            {
                bool nodeExists = links.outerLinks.ContainsKey(pair.Key);
                Debug.Assert(nodeExists);
                if (!nodeExists)
                {
                    Console.Error.WriteLine($"Error: unable to set link from {Id} to {pair.Key}");
                    continue;
                }
                outerLinkedNodes.Add(idToNode[pair.Key]);
            }
            outerLinkedNodes.Sort();
            foreach (Node node in outerLinkedNodes)
            {
                var link = links.outerLinks[node.Id];
                newLinkSection.Add(link.Print(LinkData.TypeSymbol.OuterLink));
                edges.Add(Edge.OuterLink(this, node));
            }

            return newLinkSection;
        }
        public List<Edge> ProcessNote(Dictionary<string, Node> idToNode)
        {
            List<string> originalText, oldLinkSection, noteSection;
            SplitText(out originalText, out noteSection, out oldLinkSection);

            LinksInfo links = ProcessLinks(noteSection, oldLinkSection);

            List<string> newLinkSection = FormLinkSectionAndEdges(links, idToNode, out var edges);

            List<string> newText = new(noteSection);
            newText.Add("%%ZK:links%%");
            newText.Add("***");
            newText.AddRange(newLinkSection);

            if (!Enumerable.SequenceEqual(originalText, newText))
            {
                File.WriteAllLines(NotePath, newText);
                Parameters.IncrementNoteCounter();
            }
            return edges;
        }

        public int CompareTo(Node? other)
        {
            if (other is null) return 1;
            var res = IdData.CompareTo(other.IdData);
            return res;
        }
        public override string ToString()
        {
            return NoteName;
        }
    }
}
