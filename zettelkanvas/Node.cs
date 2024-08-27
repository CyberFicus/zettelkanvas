using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace zettelkanvas
{
    internal class Node : IComparable<Node>
    {
        [JsonIgnore]
        public static Parameters? Parameters { get; set; } = null;

        [JsonPropertyName("id")]
        public string Id { get; private set; }

        [JsonPropertyName("file")]
        public string FileProperty { get; private set; }

        [JsonPropertyName("x")]
        public int OutputX { get { return X * 600; } }
        
        [JsonPropertyName("y")]
        public int OutputY { get { return Y * 600; } }

        [JsonPropertyName("type")]
        public string Type { get { return "file"; } }

        [JsonPropertyName("width")]
        public int Width { get { return 400; } }

        [JsonPropertyName("height")]
        public int Height { get { return 400; } }

        [JsonIgnore]
        public string NotePath { get; private set; }
        [JsonIgnore]
        public IdData IdData { get; set; }
        [JsonIgnore]
        public string NoteName { get; private set; }
        /// <summary>
        /// Node id -> link
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, LinkData> Links;
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

            List<string> noteText = [];
            Links = new Dictionary<string, LinkData>();
            bool linkSectionStarted = false;
            foreach (string line in File.ReadLines(notePath))
            {
                if (!linkSectionStarted)
                {
                    if (!IsRoot && line.Contains("%%ZK:\\n%%")) IsRoot = true;

                    string noteLine = line;

                    string[] split = line.Split("%%ZK:links%%");
                    if (split.Length > 1)
                    {
                        noteLine = split[0];
                        TryAddFirstLink(split[1]);
                        
                        noteLine += "%%ZK:links%%\n***";

                        linkSectionStarted = true;
                    }

                    noteText.Add(noteLine);
                }
                else
                    TryAddFirstLink(line);
            }
            if (!linkSectionStarted)
                noteText.Add("%%ZK:links%%\n***");

            foreach (string line in noteText)
                TryAddLinks(line);
            
            File.WriteAllLines(notePath, noteText);
        }

        /// <summary>
        /// To parse link section of note for already processed links 
        /// </summary>
        private void TryAddFirstLink(string line)
        {
            Match linkMatch = Regexes.LinkRegex().Match(line);
            if (!linkMatch.Success) return;
            // there is a link in line

            string parsedLinkText = linkMatch.Value[2..^2].Trim();
            var noteNameMatch = Regexes.NoteName().Match(parsedLinkText);
            if (!noteNameMatch.Success) return;
            var linkedNoteName = noteNameMatch.Value;
            // it is a link to a supposedly zettelkasten note

            var idMatch = Regexes.IdRegex().Match(linkedNoteName);
            if (!idMatch.Success) return;
            string linkedNoteId = idMatch.Value;
            if (Links.ContainsKey(linkedNoteId)) return;
            // linked note has a valid id

            var newLinkText = linkedNoteName;
            bool linkHasAlias = parsedLinkText.Contains('|');
            bool linkedNoteHasLongName = (newLinkText.Trim().Length > linkedNoteId.Trim().Length);
            if (!linkHasAlias && linkedNoteHasLongName) newLinkText += $"|{linkedNoteId}";
            // text for new link is ready

            var matchComment = Regexes.LinkComment().Match(line);
            var linkComment = (matchComment.Success) ? matchComment.Value[1..^1] : "-";
            // link comment is ready

            LinkData link =  new(newLinkText, linkComment);
            Links.Add(linkedNoteId, link);
        }
        /// <summary>
        /// To parse note's text for unprocessed links
        /// </summary>
        private void TryAddLinks(string line)
        {
            foreach (Match linkMatch in Regexes.LinkRegex().Matches(line))
            {
                // there is a link in line
                
                string parsedLinkText = linkMatch.Value[2..^2].Trim();
                var noteNameMatch = Regexes.NoteName().Match(parsedLinkText);
                if (!noteNameMatch.Success) continue;
                var linkedNoteName = noteNameMatch.Value;
                // it is a link to a supposedly zettelkasten note
                
                var idMatch = Regexes.IdRegex().Match(linkedNoteName);
                if (!idMatch.Success) continue;
                string linkedNoteId = idMatch.Value;
                if (Links.ContainsKey(linkedNoteId)) continue;
                // linked note has a valid id
                
                var newLinkText = linkedNoteName;
                bool linkedNoteHasLongName = (newLinkText.Trim().Length > linkedNoteId.Trim().Length);
                if (linkedNoteHasLongName)
                    newLinkText += $"|{linkedNoteId}";
                // text for new link is ready

                var aliasMatch = Regexes.LinkAlias().Match(linkMatch.Value);
                var autoLinkComment = "-";
                if (aliasMatch.Success) 
                    autoLinkComment = aliasMatch.Value[1..^2];
                // comment for new link is ready

                LinkData link = new(newLinkText, autoLinkComment);
                Links.Add(linkedNoteId, link);
            }
        }
        public void TryAddLinksToConnectedNodes()
        {
            string newLinkText; 
            if (Parent is not null && !Links.ContainsKey(Parent.Id))
            {
                newLinkText = (Parent.NoteName.Length > Parent.Id.Length) ? $"{Parent.NoteName}|{Parent.Id}" : Parent.NoteName;
                Links.Add(Parent.Id, new LinkData(newLinkText));
            }

            if (Next is not null && !Links.ContainsKey(Next.Id))
            {
                newLinkText = (Next.NoteName.Length > Next.Id.Length) ? $"{Next.NoteName}|{Next.Id}" : Next.NoteName;
                Links.Add(Next.Id, new LinkData(newLinkText));
            }

            foreach (Node branch in Branches)
            {
                if (!Links.ContainsKey(branch.Id))
                {
                    newLinkText = (branch.NoteName.Length > branch.Id.Length) ? $"{branch.NoteName}|{branch.Id}" : branch.NoteName;
                    Links.Add(branch.Id, new LinkData(newLinkText));
                }
            }
        }
        public LinkData? RetrieveLink(string nodeId)
        {
            LinkData? res = null;
            if (Links.TryGetValue(nodeId, out res))
            {
                Links.Remove(nodeId);
                return res;
            }
            return null;
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
