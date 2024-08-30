using System.Text.Json.Serialization;

using zettelkanvas.Nodes;

namespace zettelkanvas.Edges
{
    internal class Edge
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("fromNode")]
        public string FromNode { get; set; }
        [JsonPropertyName("fromSide")]
        public string FromSide { get; set; }
        [JsonPropertyName("toNode")]
        public string ToNode { get; set; }
        [JsonPropertyName("toSide")]
        public string ToSide { get; set; }

        [JsonIgnore]
        EdgeArrowType EdgeMode { get; set; } = EdgeArrowType.Forward;
        [JsonPropertyName("fromEnd")]
        public string? FromEnd { get { return (2 & (int)EdgeMode) != 0 ? "arrow" : null; } }
        [JsonPropertyName("toEnd")]
        public string? ToEnd { get { return (1 & (int)EdgeMode) != 0 ? null : "none"; } }

        public Edge(string fromNode, string fromSide, string toNode, string toSide)
        {
            Id = fromNode + "to" + toNode;
            FromNode = fromNode;
            FromSide = fromSide;
            ToNode = toNode;
            ToSide = toSide;
        }
        public static Edge TreeLink(string fromNodeId, string toNodeId)
        {
            return new Edge(fromNodeId, "right", toNodeId, "left");
        }
        public static Edge OuterLink(Node fromNode, Node toNode)
        {
            string fromSide = fromNode.Y <= toNode.Y ? "bottom" : "top";
            string toSide = fromNode.Y < toNode.Y ? "top" : "bottom";

            var edge = new Edge(fromNode.Id, fromSide, toNode.Id, toSide);
            edge.EdgeMode = EdgeArrowType.None;
            return edge;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
