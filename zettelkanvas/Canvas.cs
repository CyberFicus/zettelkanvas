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

        public Canvas(List<Node> nodes, Dictionary<string, Node> idToNode) {
            Nodes = nodes;
            Edges = [];

            foreach (var node in Nodes)
            {
                List<string> linkSection = [];

                LinkData? link;
                if (node.Parent is not null)
                {
                    Edges.Add(Edge.TreeLink(node.Parent.Id, node.Id));
                    link = node.RetrieveLink(node.Parent.Id);
                    linkSection.Add(link.Print(LinkData.TypeSymbol.PrevLink));
                }
                if (node.Next is not null)
                {
                    link = node.RetrieveLink(node.Next.Id);
                    linkSection.Add(link.Print(LinkData.TypeSymbol.NextLink));
                }
                foreach (var branch in node.Branches)
                {
                    link = node.RetrieveLink(branch.Id);
                    linkSection.Add(link.Print(LinkData.TypeSymbol.BranchLink));
                }

                var outerLinks = new List<Node>();
                foreach (string nodeId in node.Links.Keys)
                {
                    idToNode.TryGetValue(nodeId, out var newNode);
                    outerLinks.Add(newNode);
                }
                outerLinks.Sort();
                foreach (var listNode in outerLinks)
                {
                    Edges.Add(Edge.OuterLink(node, listNode));
                    link = node.RetrieveLink(listNode.Id);
                    linkSection.Add(link.Print(LinkData.TypeSymbol.OuterLink));
                }

                File.AppendAllLines(node.NotePath, linkSection);
            }
        }
    }
}
