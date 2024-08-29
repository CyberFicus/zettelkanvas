using System.Text.Json;
using System.Text.Json.Serialization;

namespace zettelkanvas
{
    internal class Canvas
    {
        [JsonPropertyName("nodes")]
        public List<string> Nodes { get; set; }
        [JsonPropertyName("edges")]
        public List<string> Edges { get; set; }

        public Canvas(List<Node> nodes, List<Edge> edges) {
            var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            Nodes = [];
            Edges = [];

            foreach (Node node in nodes)
                Nodes.Add(JsonSerializer.Serialize(node, jsonOptions));
            foreach (Edge edge in edges)
                Edges.Add(JsonSerializer.Serialize(edge, jsonOptions));
        }
    }
}
