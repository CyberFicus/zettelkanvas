using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Zettelkanvas.Edges;
using Zettelkanvas.Nodes;
using Zettelkanvas.Static;

namespace Zettelkanvas
{
    internal static class Program
    {
        private static List<Node> GetNodesFromDir(string dirRelativePath, out Dictionary<string, Node> idToNode)
        {
            List<Node> nodes = [];
            idToNode = [];
            int pathLength = dirRelativePath.Length;
            
            foreach (string filePath in Directory.GetFiles(dirRelativePath))
            {
                if (!filePath.EndsWith(".md")) continue;
                string noteName = filePath.Substring(pathLength + 1, filePath.Length - pathLength - 4);
                Match idMatch = Regexes.IdRegex().Match(noteName);
                if (!idMatch.Success) continue;
                string id = idMatch.Value;
                if (idToNode.ContainsKey(id)) continue;

                Node newNode = new(filePath, id, noteName);

                nodes.Add(newNode);
                idToNode.Add(id, newNode);
            }
            
            return nodes;
        }
        private static List<Node> BuildTrees(List<Node> nodes) {
            List<Node> rootNodes = [];
            nodes.Sort();
            Node? mainlineNode = nodes[0], currentNode = nodes[0];

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (i == 0 || node.IsRoot)
                {
                    rootNodes.Add(node);
                    mainlineNode = node;
                    currentNode = node;
                    continue;
                }
                while (true)
                {
                    if (node.IdData.NameParts[0].Branch == "")
                    {
                        mainlineNode.SetNext(node);
                        mainlineNode = node;
                        break;
                    }
                    var relation = currentNode.Relation(node);
                    if (relation == 0)
                    {
                        currentNode.SetNext(node);
                        break;
                    }
                    if (relation == 1)
                    {
                        currentNode.AddBranch(node);
                        break;
                    }

                    if (currentNode == mainlineNode)
                    {
                        mainlineNode.SetNext(node);
                        mainlineNode = node;
                        break;
                    }
                    if (currentNode.Parent is not null)
                        currentNode = currentNode.Parent;
                    else
                        throw new NotImplementedException();
                }
                currentNode = node;
            }

            return rootNodes;
        }

        static void Main(string[] args)
        {
#if DEBUG
            args = [
                "testdir\\testdirZK",
                "testdir\\zettelkanvas"
            ];
#endif
            try
            {
                Parameters.SetParameters(args);
            } catch
            {
                Console.WriteLine("Programm execution interrupted");
                return;
            }

            List<Node> nodeList = GetNodesFromDir(Parameters.TargetDirPath, out Dictionary<string, Node> idToNode);
            
            List<Node> rootNodes = BuildTrees(nodeList);

            rootNodes[0].Arrange(out int length, out int height);
            for (int i = 1; i < rootNodes.Count; i++)
            {
                rootNodes[i].MoveFromNode(rootNodes[i - 1], 0, height);
                rootNodes[i].Arrange(out length, out height);
            }

            List<Edge> edgeList = [];
            foreach (var node in nodeList)
            {
                edgeList.AddRange(node.ProcessNote(idToNode));
            }

            var Canvas = new Canvas(nodeList, edgeList);

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var canvasJson = JsonSerializer.Serialize(Canvas, jsonOptions);
            var canvasFormattedJson = canvasJson.Replace("\\u0022", "\"").Replace("\"{", "{").Replace("}\"", "}").Replace("  ", "\t").Replace(": [", ":[");

            File.WriteAllBytes(
                Parameters.OutputFilePath,
                Encoding.UTF8.GetBytes(canvasFormattedJson)
            );

            Console.WriteLine($"zettelkanvas: {Parameters.UpdatedNoteCouner} notes updated");
        }
    }
}
