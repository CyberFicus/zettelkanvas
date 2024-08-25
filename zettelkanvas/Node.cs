using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;


namespace zettelkanvas
{
    internal class Node : IComparable<Node>
    {
        [JsonIgnore]
        public PositionData PositionData { get; set; }
        
        [JsonIgnore]
        public string NoteName { get; private set; }

        [JsonIgnore]
        public string PathToNote {  get; private set; }

        [JsonIgnore]
        public Node? Parent { get; private set; }
        
        [JsonIgnore]
        public Node? Next { get; private set; }
        
        [JsonIgnore]
        public List<Node> Branches { get;}

        [JsonPropertyName("id")]
        public string Position { get; private set; }
        
        [JsonPropertyName("file")]
        public string FormattedFilePath { get; private set; }
        
        [JsonIgnore]
        public int X { get; set; }        
        [JsonIgnore]
        public int Y { get; set; }
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
        public bool IsRoot { get; private set; }

        public static int PathLength = Program.PathToTargetDir.Length;
        public static Regex PositionRegex = new Regex(@"\d+[a-z\d]*\b", RegexOptions.Compiled);
        public Node(string filePath) {
            if (!filePath.EndsWith(".md")) throw new Exception("Unsuitable file format");

            PathToNote = filePath;
            NoteName = filePath.Substring(PathLength + 1, filePath.Length - PathLength - 4);
            FormattedFilePath = Program.BasePathForNode + NoteName + ".md";
            var match = PositionRegex.Match(NoteName);
            if (!match.Success) { throw new Exception("Filename does not contain zettelkasten position"); }
            Position = match.Value;
            PositionData = new PositionData(Position);

            var Text = File.ReadAllText(filePath);
            IsRoot = Text.Contains("%%ZK:\\n%%");

            X = 0;
            Y = 0;
            Branches = new List<Node>();

#if DEBUG
            if (NoteName == "3a1")
            {
                ProcessLinks(out var str, out var links);
            }
#endif
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
            var res = PositionData.CompareTo(other.PositionData);
            return res;
        }

        public static Regex LinkRegex = new Regex(@"\[\[(\d|[a-zA-Z])+[\w\s]*(\|(.)*)*\]\]", RegexOptions.Compiled);
        public static Regex noteNameFromLinkRegex = new Regex(@"(\d|[a-zA-Z])+[\w\s]*", RegexOptions.Compiled);
        public static Regex LinkCommentaryRegex = new Regex(@"(`|\|).+(`|\]\])", RegexOptions.Compiled);

        public void ProcessLinks(out string noteText, out Dictionary<string, LinkData> links )
        {
            string fullText = File.ReadAllText(PathToNote);
            links = new Dictionary<string, LinkData>();
            var split = fullText.Split("%%ZK:links%%");
            noteText = split[0];
            if (!noteText.EndsWith("\n")) noteText += "\n";
            noteText += "%%ZK:links%%\n***\n";

            if (split.Length > 1)
            {
                foreach (var line in split[1].Split("\n")) {
                    var matchLink = LinkRegex.Match(line);
                    if (!matchLink.Success) continue;
                    string linkText = noteNameFromLinkRegex.Match(matchLink.Value).Value.Trim();

                    var matchComm = LinkCommentaryRegex.Match(line);
                    var link = new LinkData(linkText, (matchComm.Success) ? matchComm.Value.Substring(1, matchComm.Value.Length-2) : "-");

                    links.Add(link.LinkText, link);
                }
            }

            foreach (Match match in LinkRegex.Matches(noteText))
            {
                string linkText = noteNameFromLinkRegex.Match(match.Value).Value.Trim();
                var autoCommentMatch = LinkCommentaryRegex.Match(match.Value);
                string? autoComment = null;
                if (autoCommentMatch.Success)
                {
                    autoComment = autoCommentMatch.Value;
                    autoComment = autoComment.Substring(1, autoComment.Length - 3);
                }
                if (!links.ContainsKey(linkText))
                    links.Add(linkText, new LinkData(linkText, (autoComment is not null) ? autoComment : "?"));
            }

            static void TryAdd(string linkText, Dictionary<string, LinkData> links)
            {
                if (!links.ContainsKey(linkText))
                    links.Add(linkText, new LinkData(linkText));
            }

            if (Parent is not null)
                TryAdd(Parent.NoteName, links);
            if (Next is not null)
                TryAdd(Next.NoteName, links);
            foreach(var branch in Branches)
            {
                TryAdd(branch.NoteName, links);
            }
        }
    }
}
