using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using zettelkanvas;
using Zettelkanvas.Edges;
using Zettelkanvas.Nodes.Ids;
using Zettelkanvas.Nodes.Links;
using Zettelkanvas.Static;

namespace Zettelkanvas.Nodes
{
    internal class Node : IComparable<Node>
    {
        public string Id { get; private set; }
        public string FileProperty { get; private set; }
        public string Type { get { return "file"; } }
        public int OutputX { get { return X * (int) (Parameters.NodeWidth + Parameters.NodeDistance); } }
        public int OutputY { get { return Y * (int) (Parameters.NodeHeight + Parameters.NodeDistance); } }
        public uint Width { get { return Parameters.NodeWidth; } }
        public uint Height { get { return Parameters.NodeHeight; } }
        public CanvasColor? Color { get; private set; } = Parameters.DefaultNodeColor;

        public string Print()
        {
            var idString = $"\"id\":\"{Id}\"";
            var fileString = $",\"file\":\"{FileProperty}\"";
            var typeString = $",\"type\":\"file\"";
            var xString = $",\"x\":{OutputX}";
            var yString = $",\"y\":{OutputY}";
            var widthString = $",\"width\":{Width}";
            var heightString = $",\"height\":{Height}";
            var colorString = (Color is null) ? "" : $",\"color\":\"{Color}\"";
            return $"{{{idString}{fileString}{typeString}{xString}{yString}{widthString}{heightString}{colorString}}}";
        }

        [JsonIgnore]
        public string NotePath { get; private set; }
        [JsonIgnore]
        public IdData IdData { get; set; }
        [JsonIgnore]
        public string NoteName { get; private set; }
        [JsonIgnore]
        public bool IsRoot { get; private set; } = false;
        [JsonIgnore]
        public bool InverseArrangement { get; private set; } = false; 
        [JsonIgnore]
        public string? LinkAlias { get; private set; } = null;
        [JsonIgnore]
        public int X { get; set; } = 0;
        [JsonIgnore]
        public int Y { get; set; } = 0;
        
        [JsonIgnore]
        public Node? Parent { get; private set; }
        [JsonIgnore]
        public Node? Next { get; private set; }
        [JsonIgnore]
        public List<Node> Branches { get; } = new List<Node>();

        public Node(string notePath, string id, string noteName)
        {
            NotePath = notePath;
            Id = id;
            IdData = new(id);
            NoteName = noteName;
#pragma warning disable CS8602
            FileProperty = Parameters.FilePropertyBase + NoteName + ".md";
#pragma warning restore CS8602
            var properties = ReadProperties();

            string? GetProperty(string key)
            {
                if (!properties.ContainsKey(key)) return null;
                var list = properties[key];
                if (list.Count == 0) return null;
                string result = list[0];
                if (result.StartsWith("'") && result.EndsWith("'"))
                    result = result[1..^1];
                return result;
            }
            
            LinkAlias = GetProperty("aliases");
            
            string? rootValue = GetProperty(Parameters.RootNotePropertyName);
            if (int.TryParse(id, out _) && (rootValue == "true" || rootValue == "1"))
                IsRoot = true;

            string? inverseArrangementValue = GetProperty(Parameters.InverseArrangementPropertyName);
            if (inverseArrangementValue == "true" || inverseArrangementValue == "1")
                InverseArrangement = true;
        }
        private Dictionary<string, List<string>> ReadProperties()
        {
            Dictionary<string, List<string>> result = new();

            string[] lines = File.ReadAllLines(this.NotePath);
            int i = 1;
            while (i < lines.Length && !lines[i - 1].Contains("---"))
                i++;

            if (i == lines.Length) return result;

            string currentLongProperty = "";
            bool readingLingProperty = false;

            while (i < lines.Length && !lines[i].Contains("---"))
            {
                string line = lines[i];
                Match match = Regexes.PropertyOneLine().Match(line);
                if (match.Success)
                {
                    result[match.Groups[1].Value] = new List<string>([match.Groups[4].Value]);
                    readingLingProperty = false;
                    i++;
                    continue;
                }

                match = Regexes.PropertyMultiLineStart().Match(line);
                if (match.Success)
                {
                    currentLongProperty = match.Groups[1].Value;
                    result[currentLongProperty] = new List<string>();
                    readingLingProperty = true;
                    i++;
                    continue;
                }

                if (!readingLingProperty)
                {
                    i++;
                    continue;
                }
                match = Regexes.PropertyMultiLinePart().Match(line);
                if (match.Success)
                {
                    result[currentLongProperty].Add(match.Groups[1].Value);
                }
                i++;
            }


            return result;
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
        public void ShiftDown(bool[,] map)
        {
            if (map[X, Y + 1]) throw new NotImplementedException();

            map[X, Y + 1] = true;
            map[X, Y] = false;

            Y++;

            for (int i = Branches.Count - 1; i >= 0; i--)
                Branches[i].ShiftDown(map);
            if (Next is null) return;
            Next.ShiftDown(map);
        }
        public bool TryShiftUp(bool[,] map)
        {
            bool result = true;

            bool NoNextEdgeCollision()
            {
                if (Next is null) return true;
                if (Next.X - X == 1) return true;
                for (int x = X+1; x < Next.X; x++)
                {
                    if (map[x, Y-1]) return false;
                }
                return true;
            }

            if (map[X, Y - 1] || !NoNextEdgeCollision())
            {
                result = false;
            } else
            {
                map[X, Y] = false;
                map[X, Y-1] = true;
                Y--;

                if (Next is not null)
                {
                    result = Next.TryShiftUp(map);
                    if (!result)
                    {
                        Y++;
                        map[X, Y - 1] = false;
                        map[X, Y] = true;
                        return result;
                    }
                }
                for (int i = 0; i < Branches.Count; i++)
                {
                    result = Branches[i].TryShiftUp(map);
                    if (!result)
                    {
                        i--;
                        while (i >= 0)
                        {
                            Branches[i].ShiftDown(map);
                            i--;
                        }

                        if (Next is not null)
                            Next.ShiftDown(map);

                        Y++;
                        map[X, Y - 1] = false;
                        map[X, Y] = true;

                        return result;
                    }
                }
            }

            return result;
        }
        public void Shrink(bool[,] map)
        {
            if (Next is not null)
                Next.Shrink(map);
            foreach (var branch in Branches)
                branch.Shrink(map);

            int GetMinYForRoot()
            {
                bool LineIsFree(int y)
                {
                    if (y < 0) return false;
                    for (int x = 0; x < map.GetLength(0); x++)
                    {
                        if (map[x, y])
                            return false;
                    }
                    return true;
                }
                int minY = Y-1;
                while (LineIsFree(minY) && minY >=0)
                    minY--;

                return minY+1;
            }

            int minY = (Parent is null) ? GetMinYForRoot() : Parent.Y+1;

            bool res = true;
            while (Y > minY && res)
                res = TryShiftUp(map);
        }

        public void Arrange(out int length, out int height)
        {
            if (Parameters.UseLongArrange ^ (this.InverseArrangement && Parameters.AllowInverseArrangement))
                ArrangeLong(out length, out height);
            else 
                ArrangeWide(out length, out height);
        }
        private void ArrangeLong(out int length, out int height)
        {
            length = 1; 
            height = 1;
            int lengthBuf = 0,
                heightBuf = 0;
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
                length += lengthBuf; 
                height = int.Max(height, heightBuf);
            }
        }
        private void ArrangeWide(out int length, out int height)
        {
            length = 1;
            height = 1;
            int lengthBuf = 0, 
                heightBuf = 0;
            
            if (Next is not null)
            {
                Next.MoveFromNode(this, 1, 0);
                Next.Arrange(out lengthBuf, out heightBuf);
                length += lengthBuf;
                height = int.Max(height, heightBuf);
            }

            for (int i = 0; i < Branches.Count; i++)
            {
                Branches[i].MoveFromNode(this, 1, height);
                Branches[i].Arrange(out lengthBuf, out heightBuf);
                height += heightBuf;
                length = int.Max(length, lengthBuf+1);
            }
        }

        public int Relation(Node node)
        {
            return IdData.Relation(this.IdData, node.IdData);
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
        private Dictionary<string, LinkData> GetLinks(List<string> noteSection, List<string> linkSection)
        {
            Dictionary<string, LinkData> links = [];

            foreach (string line in noteSection) { 
                var lineLinks = LinkData.ProcessNoteSectionLine(line);
                foreach (var link in lineLinks)
                    links.TryAdd(link.LinkedNoteId, link);
            }

            if (Parent is not null)
                links.TryAdd(Parent.Id, new LinkData(Parent));
            if (Next is not null)
                links.TryAdd(Next.Id, new LinkData(Next));
            foreach (var branch in Branches)
                links.TryAdd(branch.Id, new LinkData(branch));

            foreach (string line in linkSection) { 
                var link = LinkData.ProcessLinkSectionLine(line);
                if (link is not null && links.ContainsKey(link.LinkedNoteId))
                    links[link.LinkedNoteId] = link;
            }

            return links;
        }
        private List<string> FormLinkSectionAndEdges(Dictionary<string, LinkData> links, Dictionary<string, Node> idToNode, out List<Edge> edges)
        {
            List<string> newLinkSection = [];
            edges = new();

            LinkData Retrieve(string id)
            {
                LinkData res = links[id];
                links.Remove(id);
                return res;
            }

            if (Parent is not null)
            {
                var link = Retrieve(Parent.Id);
                newLinkSection.Add(link.Print(LinkType.ParentLink));
                edges.Add(Edge.TreeLink(Parent.Id, Id));
            }

            if (Next is not null)
            {
                var link = Retrieve(Next.Id);
                newLinkSection.Add(link.Print(LinkType.NextLink));
            }

            foreach (Node branch in Branches)
            {
                var link = Retrieve(branch.Id);
                newLinkSection.Add(link.Print(LinkType.BranchLink));
            }

            List<Node> outerLinkedNodes = [];
            foreach (var pair in links)
                if (idToNode.ContainsKey(pair.Key))
                    outerLinkedNodes.Add(idToNode[pair.Key]);
            outerLinkedNodes.Sort();

            foreach (Node node in outerLinkedNodes)
            {
                var outerLink = links[node.Id];
                newLinkSection.Add(outerLink.Print(LinkType.OuterLink));
                edges.Add(Edge.OuterLink(this, node));
            }

            return newLinkSection;
        }
        public List<Edge> ProcessNote(Dictionary<string, Node> idToNode)
        {
            List<string> originalText, oldLinkSection, noteSection;
            SplitText(out originalText, out noteSection, out oldLinkSection);

            Dictionary<string, LinkData> links = GetLinks(noteSection, oldLinkSection);

            if (Parameters.LinkAutoAlias)
            {
                foreach (var pair in links) 
                {
                    LinkData link = pair.Value;
                    if (!links.ContainsKey(link.LinkedNoteId))
                        continue;
                    Node linkedNote = idToNode[link.LinkedNoteId];
                    if (linkedNote.LinkAlias is null)
                        continue;
                    link.Comment = linkedNote.LinkAlias;
                }
            }

            List<string> newLinkSection = FormLinkSectionAndEdges(links, idToNode, out var edges);

            List<string> newText = new(noteSection);
            newText.Add("%%ZK:links%%");
            newText. Add("***");
            newText.AddRange(newLinkSection);

            if (!originalText.SequenceEqual(newText))
            {
                using (StreamWriter writer = new(NotePath))
                {
                    writer.NewLine = "\n";
                    foreach (string line in newText)
                        writer.WriteLine(line);
                }
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
            return $"{NoteName} ({X},{Y})";
        }
    }
}
