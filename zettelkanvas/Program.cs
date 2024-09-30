using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                Match idMatch = Regexes.IdInFileName().Match(noteName);
                if (!idMatch.Success) continue;
                string id = idMatch.Value.Trim();
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
        private static void ArrangeNodes(List<Node> rootNodes)
        {
            rootNodes[0].Arrange(out int length, out int height);
            for (int i = 1; i < rootNodes.Count; i++)
            {
                rootNodes[i].MoveFromNode(rootNodes[i - 1], 0, height);
                rootNodes[i].Arrange(out length, out height);
            }
        }
        private static void ShrinkTrees(List<Node> nodes, List<Node> rootNodes, Dictionary<string, Node> idToNode)
        {
            int maxX = 0, maxY = 0;
            foreach (Node node in nodes)
            {
                if (node.X > maxX) maxX = node.X;
                if (node.Y > maxY) maxY = node.Y;
            }

            bool[,] map = new bool[maxX + 1, maxY + 1];
            foreach (Node node in nodes)
                map[node.X, node.Y] = true;

            foreach (Node root in rootNodes)
                root.Shrink(map);

        }
        private static List<Edge> BuildEdges(List<Node> nodeList, Dictionary<string, Node> idToNode)
        {
            List<Edge> edgeList = [];
            foreach (var node in nodeList)
            {
                edgeList.AddRange(node.ProcessNote(idToNode));
            }
            return edgeList;
        }
        private static List<string> BuildCanvas(List<Node> nodes, List<Edge> edges)
        {
            List<string> canvas = ["{", "\t\"nodes\":["];

            var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            foreach (Node node in nodes)
                canvas.Add("\t\t" + node.Print() + ",");
            if (nodes.Count > 0)
                canvas[canvas.Count-1] = canvas[canvas.Count-1][0..^1];

            canvas.AddRange(["\t],", "\t\"edges\":["]);

            foreach (Edge edge in edges)
                canvas.Add("\t\t" + edge.Print() + ",");
            if (edges.Count > 0)
                canvas[canvas.Count-1] = canvas[canvas.Count-1][0..^1];

            canvas.AddRange(["\t]", "}"]);

            return canvas;
        }
        static void Main(string[] args)
        {
#if DEBUG
            Directory.SetCurrentDirectory("..\\..\\..\\..\\ZKTest");
            args = [
                "testdir\\testdirZK",
                "testdir\\zettelkanvas",
                "zettelkanvas.config"
            ];
#endif
            try
            {
                Parameters.SetParameters(args);
            } catch (Exception e)
            {
                if (e.Message != "")
                    Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine("Programm execution interrupted.");
                return;
            }

            List<Node> nodeList = GetNodesFromDir(Parameters.TargetDirPath, out Dictionary<string, Node> idToNode);
            if (nodeList.Count == 0) {
                Console.Error.WriteLine("No suitable notes found");

                List<string> emptyCanvas = BuildCanvas(nodeList, new List<Edge>());

                using (StreamWriter writer = new(Parameters.OutputFilePath))
                {
                    writer.NewLine = "\n";
                    foreach (string line in emptyCanvas)
                        writer.WriteLine(line);
                }
                return;
            }
            List<Node> rootNodes = BuildTrees(nodeList);

            ArrangeNodes(rootNodes);

            if (Parameters.ShrinkTrees)
                ShrinkTrees(nodeList, rootNodes, idToNode);

            List<Edge> edgeList = BuildEdges(nodeList, idToNode);

            List<string> canvas = BuildCanvas(nodeList, edgeList);

            using (StreamWriter writer = new(Parameters.OutputFilePath))
            {
                writer.NewLine = "\n";
                foreach (string line in canvas)
                    writer.WriteLine(line);
            }
 
            Console.WriteLine($"zettelkanvas: {Parameters.UpdatedNoteCouner} notes updated");
        }
    }
}
