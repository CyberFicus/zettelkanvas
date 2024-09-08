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
            if (Parameters.UseLongArrange)
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
                Branches[i].ArrangeLong(out lengthBuf, out heightBuf);
                height += heightBuf;
                length = int.Max(length, lengthBuf);
            }

            if (Next is not null)
            {
                Next.MoveFromNode(this, length, 0);
                Next.ArrangeLong(out lengthBuf, out heightBuf);
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
                Next.ArrangeWide(out lengthBuf, out heightBuf);
                length += lengthBuf;
                height = int.Max(height, heightBuf);
            }

            for (int i = 0; i < Branches.Count; i++)
            {
                Branches[i].MoveFromNode(this, 1, height);
                Branches[i].ArrangeWide(out lengthBuf, out heightBuf);
                height += heightBuf;
                length = int.Max(length, lengthBuf);
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

            List<string> newLinkSection = FormLinkSectionAndEdges(links, idToNode, out var edges);

            List<string> newText = new(noteSection);
            newText.Add("%%ZK:links%%");
            newText. Add("***");
            newText.AddRange(newLinkSection);

            if (!originalText.SequenceEqual(newText))
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
