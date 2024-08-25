using System.Text.Json.Serialization;
using System.Text.RegularExpressions;


namespace zettelkanvas
{
    internal class Node : IComparable<Node>
    {
        [JsonIgnore]
        public PositionData PositionData { get; set; }
        
        [JsonIgnore]
        public string NoteName { get; private set; }
        
        [JsonIgnore]
        public List<string>? LinkedNodePostions { get; private set; }
        
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
        
        [JsonPropertyName("x")]
        public int X { get; set; }
        
        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("type")]
        public string Type { get { return "file"; } }

        [JsonPropertyName("width")]
        public int Width { get { return 400; } }

        [JsonPropertyName("height")]
        public int Height { get { return 400; } }

        private static int PathLength = Program.PathToTargetDir.Length;
        private static Regex PositionRegex = new Regex(@"\d+[a-z\d]*\b", RegexOptions.Compiled);
        public Node(string filePath) {
            if (!filePath.EndsWith(".md")) throw new Exception("Unsuitable file format");
            
            NoteName = filePath.Substring(PathLength + 1, filePath.Length - PathLength - 4);
            FormattedFilePath = Program.BasePathForNode + NoteName + ".md";
            var match = PositionRegex.Match(NoteName);
            if (!match.Success) { throw new Exception("Filename does not contain zettelkasten position"); }
            Position = match.Value;
            PositionData = new PositionData(Position);
            X = 0;
            Y = 0;
            Branches = new List<Node>();
        }
        private static Regex linkRegex = new Regex(@"\[\[(\d|[a-z])+(\|(.)*)*\]\]", RegexOptions.Compiled);
        private static Regex getLinkedFileName = new Regex(@"(\d|[a-z])+", RegexOptions.Compiled);
        private static List<string> GetLinksFromFile(string path)
        {
            var res = new List<string>();
            using (var fileInput = new StreamReader(path))
            {
                string text = fileInput.ReadToEnd();
                foreach (Match match in linkRegex.Matches(text))
                {
                    string processedMatch = getLinkedFileName.Match(match.Value).Value;
                    res.Add(processedMatch);
                }
            }
            return res;
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
        /*
        public void AddToMap(Canvas map, Dictionary<string, Node> nameToNode)
        {
            map.nodes.Add(Node);

            Node fromNode;
            foreach (string link in Node.LinkedNodePostions)
            {
                if (link == Node.Position) continue;
                if (Parent is not null && link == Parent.Node.Position)
                {
                    map.edges.Add(new Edge(link, Node.Position));
                    continue;
                }

                if (nameToNode.TryGetValue(link, out fromNode))
                {
                    map.edges.Add(new Edge(fromNode, Node));
                }
            }

            foreach (var branch in Branches)
            {
                branch.AddToMap(map, nameToNode);
            }
            if (Next is not null)
            {
                Next.AddToMap(map, nameToNode);
            }
        }
        */
        public static Node SetLinkedNodes(List<Node> sortedNodes)
        {
            var baseNode = sortedNodes[0]   ;
            var currentRoot = baseNode;
            var currentNode = currentRoot;
            int i = 1;
            while (i < sortedNodes.Count)
            {
                var node = sortedNodes[i];

                while (true)
                {
                    if (node.PositionData.NameParts[0].Branch == "")
                    {
                        currentRoot.SetNext(node);
                        currentRoot = node;
                        break;
                    }
                    if (PositionData.IsBranchBase(currentNode.PositionData, node.PositionData))
                    {
                        currentNode.AddBranch(node);
                        break;
                    }
                    if (PositionData.IsSuitableNext(currentNode.PositionData, node.PositionData))
                    {
                        currentNode.SetNext(node);
                        break;
                    }
                    if (currentNode == currentRoot)
                    {
                        currentRoot.SetNext(node);
                        currentRoot = node;
                        break;
                    }
                    currentNode = currentNode.Parent;
                }

                currentNode = node;
                i++;
            }
            return baseNode;
        }

        public int CompareTo(Node? other)
        {
            if (other is null) return 1;
            var res = PositionData.CompareTo(other.PositionData);
            return res;
        }
    }

}
