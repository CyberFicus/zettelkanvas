using System.Text.Json.Serialization;

using Zettelkanvas.Nodes;

namespace Zettelkanvas.Edges
{
    internal class Edge
    {
        public string Id { get; set; }
        public string FromNode { get; set; }
        public string FromSide { get; set; }
        public string ToNode { get; set; }
        public string ToSide { get; set; }
        EdgeArrowType EdgeMode { get; set; } = EdgeArrowType.Forward;
        public string? FromEnd { get { return (2 & (int)EdgeMode) != 0 ? "arrow" : null; } }
        public string? ToEnd { get { return (1 & (int)EdgeMode) != 0 ? null : "none"; } }
        public string? Color { get; private set; } = null;

        public string Print()
        {
            var idString = $"\"id\":\"{Id}\"";
            var fromNodeString = $",\"fromNode\":\"{FromNode}\"";
            var fromSideString = $",\"fromSide\":\"{FromSide}\"";
            var toNodeString = $",\"toNode\":\"{ToNode}\"";
            var toSideString = $",\"toSide\":\"{ToSide}\"";
            var fromEndString = (FromEnd is not null) ? $",\"fromEnd\":\"{FromEnd}\"" : "";
            var toEndSting = (ToEnd is not null) ? $",\"toEnd\":\"none\"" : "";
            var colorString = (Color is not null) ? $",\"color\":\"{Color}\"" : "";
            return $"{{{idString}{fromNodeString}{fromSideString}{toNodeString}{toSideString}{fromEndString}{toEndSting}{colorString}}}";
        }

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
