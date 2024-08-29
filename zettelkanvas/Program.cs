using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace zettelkanvas
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
#pragma warning disable CS8602
                    if (IdData.IsBranchBase(currentNode.IdData, node.IdData))
#pragma warning restore CS8602
                    {
                        currentNode.AddBranch(node);
                        break;
                    }

                    if (IdData.IsSuitableNext(currentNode.IdData, node.IdData))
                    {
                        currentNode.SetNext(node);
                        break;
                    }
                    if (currentNode == mainlineNode)
                    {
                        mainlineNode.SetNext(node);
                        mainlineNode = node;
                        break;
                    }
                    currentNode = currentNode.Parent;
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
            Parameters parameters;
            try
            {
                parameters = new(args);
                Node.Parameters = parameters;
            } catch
            {
                Console.WriteLine("Programm execution interrupted");
                return;
            }

            List<Node> nodeList = GetNodesFromDir(parameters.TargetDirPath, out Dictionary<string, Node> idToNode);
            
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
                parameters.OutputFilePath,
                Encoding.UTF8.GetBytes(canvasFormattedJson)
            );

            Console.WriteLine($"zettelkanvas: {parameters.UpdatedNoteCouner} notes updated");
        }
    }
}
