using System.Drawing;
using System.Text;
using zettelkanvas;
using Zettelkanvas.Edges;

namespace Zettelkanvas.Static
{
    internal static class Parameters
    {
        public static string TargetDirPath { get; private set; } = "";
        public static string OutputFilePath { get; private set; } = "";
        public static string FilePropertyBase { get; private set; } = "";
        public static string RootNodeIndicator { get; private set; } = "!";
        public static int UpdatedNoteCouner { get; private set; } = 0;
        public static void IncrementNoteCounter()
        {
            UpdatedNoteCouner++;
        }

        public static uint NodeWidth { get; private set; } = 400;
        public static uint NodeHeight { get; private set; } = 400;
        public static uint NodeDistance { get; private set; } = 200;
        public static bool UseLongArrange { get; private set; } = false;

        public static EdgeArrowType DefaultArrow { get; private set; } = EdgeArrowType.Forward;
        public static EdgeArrowType TreeLinkArrow { get; private set; } = EdgeArrowType.TwoSided;
        public static EdgeArrowType OuterLinkArrow { get; private set; } = EdgeArrowType.Forward;

        public static CanvasColor? DefaultEdgeColor { get; private set; } = null;
        public static CanvasColor? DefaultTreeLinkColor { get; private set; } = null;
        public static CanvasColor? DefaultOuterLinkColor { get; private set; } = null;
        public static CanvasColor? DefaultNodeColor { get; private set; } = null;

        public static void SetParameters(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine(
                    "2 or 3 arguments required:\n" +
                    "1) relative path from vault root to zettelkasten directory\n" +
                    "2) relative path from vault root to output file (without .canvas format!)\n" + 
                    "3) (optional) relative path from vault root to config file\n"
                );
                throw new Exception("");
            }

            var currentDir = Directory.GetCurrentDirectory();

            TargetDirPath = Path.Combine(currentDir, args[0]);
            if (!Directory.Exists(TargetDirPath))
            {
                Console.Error.WriteLine($"Unable to locate directory {TargetDirPath}");
                throw new Exception("");
            }

            FilePropertyBase = Path.GetRelativePath(currentDir, TargetDirPath).Replace("\\", "/") + "/";

            OutputFilePath = Path.Combine(currentDir, args[1] + ".canvas");
            try
            {
                var outputFileInfo = new FileInfo(OutputFilePath);
                var outputFileStream = outputFileInfo.OpenWrite();
                outputFileStream.Write(Encoding.ASCII.GetBytes("\n"));
                outputFileStream.Dispose();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                throw new Exception("");
            }

            if (args.Length > 2)
                ProcessConfig(args[2]);

            return;
        }
        private static void ProcessConfig(string configPath)
        {
            var configFullPath = Path.Combine(Directory.GetCurrentDirectory(), configPath);
            if (!File.Exists(configFullPath))
            {
                Console.Error.WriteLine("Config file does not exist. Creating new config file");
                try
                {
                    File.WriteAllLines(configFullPath, DefaultConfigText);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Unable to create new config file: {e.Message}");
                    Console.Error.WriteLine("Running with default parameters");
                    return;
                }
                Console.Error.WriteLine("Config file was successfully created.");
                return;
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(configFullPath);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Unable to read config file: {e.Message}");
                Console.Error.WriteLine("Running with default parameters");
                return;
            }

            int failedToRecognizeCounter = 0;
            foreach (string line in lines) 
            {
                if (line.StartsWith("//"))
                    continue;

                if (Regexes.ConfigValue().Match(line).Success)
                {
                    ParseConfigValue(line);
                    continue;
                }

                if (Regexes.ConfigColor().Match(line).Success)
                {
                    ParseConfigColor(line);
                    continue;
                }

                failedToRecognizeCounter++;
                Console.Error.WriteLine($"Config line \"{line}\" was not recognized.");
            }
            if (failedToRecognizeCounter > 0)
                Console.Error.WriteLine("You can make any line into a comment by adding // at the beginning\n");
        }
        private static void ParseConfigValue(string line)
        {
            var parts = line.Split(":");
            if (parts.Length < 2)
            {
                Console.Error.WriteLine($"Unable to parse config line \"{line}\"");
                return;
            }

            string name = parts[0].Trim();
            if (parts[1].Trim() == "-")
                return;
            bool parseResult = uint.TryParse(parts[1].Trim(), out uint value);
            if (!parseResult)
            {
                Console.Error.WriteLine($"Unable to get value from config line \"{line}\"");
                return;
            }

            switch (name)
            {
                case "node_width":
                    NodeWidth = value;
                    break;
                case "node_height":
                    NodeHeight = value;
                    break;
                case "node_distance":
                    NodeDistance = value;
                    break;
                case "use_long_arrangement":
                    if (value != 0)
                        UseLongArrange = true;
                    break;
                case "default_edge_arrow":
                    if (value > 3)
                    {
                        Console.WriteLine($"Incorrect value in line \"{line}\"");
                        return;
                    }
                    DefaultArrow = (EdgeArrowType) value;
                    break;
                case "default_tree_link_arrow":
                    if (value > 3)
                    {
                        Console.WriteLine($"Incorrect value in line \"{line}\"");
                        return;
                    }
                    TreeLinkArrow = (EdgeArrowType)value;
                    break;
                case "default_outer_link_arrow":
                    if (value > 3)
                    {
                        Console.WriteLine($"Incorrect value in line \"{line}\"");
                        return;
                    }
                    OuterLinkArrow = (EdgeArrowType)value;
                    break;
                default:
                    Console.Error.WriteLine($"Unknown config value name: {name}");
                    break;
            }
        }
        private static void ParseConfigColor(string line)
        {
            var parts = line.Split(":");
            if (parts.Length < 2)
            {
                Console.Error.WriteLine($"Unable to parse config line \"{line}\"");
                return;
            }

            string name = parts[0].Trim();
            if (parts[1].Trim() == "-")
                return;
            CanvasColor? value = CanvasColor.TryBuild(parts[1]);
            if (value is null)
            {
                Console.Error.WriteLine($"Unable to get color from config line \"{line}\"");
                return;
            }

            switch (name)
            {
                case "@default_edge_color":
                    DefaultEdgeColor = value;
                    break;
                case "@default_tree_link_color":
                    DefaultTreeLinkColor = value;
                    break;
                case "@default_outer_link_color":
                    DefaultOuterLinkColor = value;
                    break;
                case "@default_node_color":
                    DefaultNodeColor = value;
                    break;
                default:
                    Console.Error.WriteLine($"Unknown color name: {name}");
                    break;
            }
        }

        private static List<string> DefaultConfigText { get; } =
            [
                "// Delete this file to reset config",
                "// Canvas parameters:",
                "node_width : 400",
                "node_height : 400",
                "node_distance : 200",
                "use_long_arrangement : 0",
                "// Arrows: 0 - none, 1 - forward, 2 - reverse, 3 - two-sided ",
                "default_edge_arrow : 1",
                "default_tree_link_arrow : 3",
                "default_outer_link_arrow : 1",
                "// Color constants: ",
                "@default_edge_color : -",
                "@default_tree_link_color : -",
                "@default_outer_link_color : -",
                "@default_node_color : -"
            ];
    }
}
