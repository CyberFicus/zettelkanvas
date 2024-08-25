using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace zettelkanvas
{
    internal static class Program
    {
        public static string PathToTargetDir { get; private set; }
        public static FileInfo OutputFile { get; private set; } 
        public static FileStream OutputFileStream { get; private set; }
        public static string BasePathForNode { get; private set; }

        public static Dictionary<string, Node> NameToNode { get; } = new Dictionary<string, Node>();
        public static Canvas Canvas { get; } = new Canvas();

        public static bool ReadAndValidateArgs(string[] args)
        {
            if (args.Length != 2) { 
                Console.Error.WriteLine(
                    "2 arguments required:\n" +
                    "1) relative path from vault root to zettelkasten directory\n" +
                    "2) relative path from vault root to output file (without .canvas format!)\n"
                );
                return false;
            }

            var currentDir = Directory.GetCurrentDirectory();
            
            PathToTargetDir = Path.Combine(currentDir, args[0]);
            if (!Directory.Exists(PathToTargetDir))
            {
                Console.Error.WriteLine($"Unable to locate directory {PathToTargetDir}");
                return false;
            }

            BasePathForNode = Path.GetRelativePath(currentDir, PathToTargetDir).Replace("\\", "/") + "/";

            string pathToOutPutFile = Path.Combine(currentDir, args[1] + ".canvas");
            try
            {
                OutputFile = new FileInfo(pathToOutPutFile);
                OutputFileStream = OutputFile.OpenWrite();
                OutputFileStream.Write(Encoding.ASCII.GetBytes("\n"));
            }
            catch (Exception ex) {
                Console.Error.WriteLine (ex.ToString());
                return false;
            }   
            
            return true;
        } 
        public static void GetAndSortNodes(string pathToDir)
        {
            foreach (string filePath in Directory.GetFiles(pathToDir))
            {
                try
                {
                    var node = new Node(filePath);
                    Canvas.Nodes.Add(node);
                    NameToNode.Add(node.NoteName, node);
                }
                catch
                {

                }
            }
            Canvas.Nodes.Sort();
        }
        public static List<Node> BuildTrees()
        {
            List<Node> rootNodes = new List<Node> ();
            var mainlineNode = Canvas.Nodes[0];
            rootNodes.Add(mainlineNode);
            var currentNode = mainlineNode;
            int i = 1;
            while (i < Canvas.Nodes.Count)
            {
                var node = Canvas.Nodes[i];
                if (node.IsRoot)
                {
                    mainlineNode = node;
                    currentNode = node;
                    rootNodes.Add(node);
                    i++;
                    continue;
                }
                while (true)
                {
                    if (node.PositionData.NameParts[0].Branch == "")
                    {
                        mainlineNode.SetNext(node);
                        mainlineNode = node;
                        break;
                    }
                    if (PositionData.IsBranchBase(currentNode.PositionData, node.PositionData))
                    {
                        currentNode.AddBranch(node);
                        break;
                    }
                    if (PositionData.IsSuitableNext(currentNode.PositionData, node.PositionData))
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
                i++;
            }
            return rootNodes;
        }
        public static void ArrangeTrees(List<Node> rootNodes)
        {
            rootNodes[0].Arrange(out int l, out int h);
            for (int i = 1; i < rootNodes.Count; i++)
            {
                rootNodes[i].MoveFromNode(rootNodes[i - 1], 0, h);
                rootNodes[i].Arrange(out l, out h);
            }
        }

        static void Main(string[] args)
        {
#if DEBUG
            args = [
                "testdir\\testdirZK",
                "testdir\\zettelkanvas"
            ];
#endif
            if (!ReadAndValidateArgs(args)) return;

            GetAndSortNodes(PathToTargetDir);

            List<Node> rootNodes = BuildTrees();

            ArrangeTrees(rootNodes);


            Canvas.Prepare();
            var jsonOfCanvas = JsonSerializer.Serialize(Canvas, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            var bytesOfCanvas = Encoding.ASCII.GetBytes(jsonOfCanvas);
            OutputFileStream.Write(bytesOfCanvas);
            OutputFileStream.Dispose();
        }
    }
}
