using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace zettelkanvas
{
    internal class Edge
    {
        [JsonPropertyName("id")]
        public string Id {  get; set; }
        [JsonPropertyName("fromNode")]
        public string fromNode { get; set; }
        [JsonPropertyName("fromSide")]
        public string fromSide { get; set; }
        [JsonPropertyName("toNode")]
        public string toNode { get; set; }
        [JsonPropertyName("toSide")]
        public string toSide { get; set; }

        public Edge(string fromNode, string toNode, string fromSide = "right")
        {
            this.Id = fromNode + "to" + toNode;
            this.fromNode = fromNode;
            this.fromSide = fromSide;
            this.toNode = toNode;
            this.toSide = "left";
        }
        public Edge(Node fromNode, Node toNode)
        {
            Id = fromNode.Position + "to" + toNode.Position;
            this.fromNode = fromNode.Position;
            this.fromSide = GetFromSide(fromNode, toNode);
            this.toNode = toNode.Position;
            this.toSide = "left";

        }
        private static string GetFromSide(Node start, Node end)
        {
            if (start.Y >= end.Y) { return "top"; }
            return "bottom";
        }
    }
}
