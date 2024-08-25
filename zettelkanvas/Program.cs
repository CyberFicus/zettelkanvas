using System.Text;
using System.Text.Json;

namespace zettelkanvas
{
    internal static class Program
    {
        public static string PathToTargetDir { get; private set; }
        public static FileInfo OutputFile { get; private set; } 
        public static FileStream OutputFileStream { get; private set; }
        public static string BasePathForNode { get; private set; }

        public static Dictionary<string, Node> PositionToNode { get; } = new Dictionary<string, Node>();
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

        static void Main(string[] args)
        {
#if DEBUG
            args = [
                "testdir\\testdirZK",
                "testdir\\zettelkanvas"
            ];
#endif
            if (!ReadAndValidateArgs(args)) return;

            var nodes = new List<Node>();
            foreach (string filePath in Directory.GetFiles(PathToTargetDir)) {
                try
                {
                    var node = new Node(filePath);
                    nodes.Add(node);
                    PositionToNode.Add(node.Position, node);
                }
                catch
                {

                }
            }
            nodes.Sort();

            var baseNode = Node.SetLinkedNodes(nodes);
            baseNode.Arrange(out int a, out int b);
            Canvas.nodes = nodes; 

            Canvas.PrepareToSerialization();
            var jsonOfCanvas = JsonSerializer.Serialize(Canvas, new JsonSerializerOptions { WriteIndented = true });
            var bytesOfCanvas = Encoding.ASCII.GetBytes(jsonOfCanvas);
            OutputFileStream.Write(bytesOfCanvas);
            OutputFileStream.Dispose();
        }
    }
}
