using System.Text.Json.Serialization;

namespace zettelkanvas
{
    internal class Edge
    {
        [JsonPropertyName("id")]
        public string Id {  get; set; }
        [JsonPropertyName("fromNode")]
        public string FromNode { get; set; }
        [JsonPropertyName("fromSide")]
        public string FromSide { get; set; }
        [JsonPropertyName("toNode")]
        public string ToNode { get; set; }
        [JsonPropertyName("toSide")]
        public string ToSide { get; set; }
        [JsonIgnore]
        public bool DisplayArrow { get; set; } = true;
        [JsonPropertyName("toEnd")]
        public string? ToEnd { get { return (DisplayArrow) ? null : "none"; } } 
        
        public Edge(string fromNode, string fromSide, string toNode, string toSide)
        {
            this.Id = fromNode + "to" + toNode;
            this.FromNode = fromNode;
            this.FromSide = fromSide;
            this.ToNode = toNode;
            this.ToSide = toSide;
        }
        public static Edge TreeLink(string fromNodeId, string toNodeId)
        {
            return new Edge(fromNodeId, "right", toNodeId, "left");
        }
        public static Edge OuterLink(Node fromNode, Node toNode)
        {
            string fromSide = (fromNode.Y <= toNode.Y) ? "bottom" : "top";
            string toSide = (fromNode.Y < toNode.Y) ? "top" : "bottom";

            var edge = new Edge(fromNode.Id, fromSide, toNode.Id, toSide);
            edge.DisplayArrow = false;
            return edge;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
