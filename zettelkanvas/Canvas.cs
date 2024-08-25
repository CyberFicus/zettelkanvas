using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace zettelkanvas
{
    internal class Canvas
    {
        [JsonPropertyName("nodes")]
        public List<Node> Nodes { get; set; }
        [JsonPropertyName("edges")]
        public List<Edge> Edges { get; set; }

        public Canvas() { 
            Nodes = new List<Node>();
            Edges = new List<Edge>();
        }

        public void Prepare()
        {
            foreach(var node in Nodes)
            {
                node.ProcessLinks(out string noteText, out Dictionary<string, LinkData> links);

                bool Retrieve(string noteName, Dictionary<string, LinkData> links, out LinkData linkData)
                {
                    if (links.TryGetValue(noteName, out linkData))
                    {
                        links.Remove(noteName);
                    }
                    return false;
                }

                LinkData link;
                if (node.Parent is not null) {
                    Retrieve(node.Parent.NoteName, links, out link);
                    noteText += link.Print(LinkData.TypeSymbol.PrevLink);
                }
                if (node.Next is not null)
                {
                    Edges.Add(Edge.TreeLink(node.Position, node.Next.Position));
                    Retrieve(node.Next.NoteName, links, out link);
                    noteText += link.Print(LinkData.TypeSymbol.NextLink);
                }
                foreach(var branch in node.Branches)
                {
                    Edges.Add(Edge.TreeLink(node.Position, branch.Position));
                    Retrieve(branch.NoteName, links, out link);
                    noteText += link.Print(LinkData.TypeSymbol.BranchLink);
                }
                var outerLinks = new List<Node>();
                foreach (string noteName in links.Keys)
                {
                    Program.NameToNode.TryGetValue(noteName, out var nodeForList);
                    if (nodeForList is null) continue;
                    outerLinks.Add(nodeForList);
                }
                outerLinks.Sort();
                foreach(var listNode in outerLinks)
                {
                    Edges.Add(Edge.OuterLink(listNode, node));
                    Retrieve(listNode.NoteName, links, out link);
                    noteText += link.Print(LinkData.TypeSymbol.OuterLink);
                }

                File.WriteAllText(node.PathToNote, noteText);
            }
        }
    }
}
