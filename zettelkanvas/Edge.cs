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
        

        public Edge(string sourcePosition, string fromSide, string destinationPosition, string toSide)
        {
            this.Id = sourcePosition + "to" + destinationPosition;
            this.FromNode = sourcePosition;
            this.FromSide = fromSide;
            this.ToNode = destinationPosition;
            this.ToSide = toSide;
        }
        public static Edge TreeLink(string parentNodePosition, string nextOrBranchPosition)
        {
            return new Edge(parentNodePosition, "right", nextOrBranchPosition, "left");
        }
        public static Edge OuterLink(Node fromNode, Node toNode)
        {
            string fromSide = (fromNode.Y <= toNode.Y) ? "bottom" : "top";
            string toSide = (fromNode.Y < toNode.Y) ? "top" : "bottom";

            var edge = new Edge(fromNode.Position, fromSide, toNode.Position, toSide);
            edge.DisplayArrow = false;
            return edge;
        }        
    }
}
