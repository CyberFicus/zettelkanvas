using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zettelkanvas
{
    internal class Parameters
    {
        public string TargetDirPath { get; private set; } = "";
        public string OutputFilePath { get; private set; } = "";
        public string FilePropertyBase { get; private set; } = "";

        public Parameters(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine(
                    "2 arguments required:\n" +
                    "1) relative path from vault root to zettelkasten directory\n" +
                    "2) relative path from vault root to output file (without .canvas format!)\n"
                ); 
                throw new Exception();
            }

            var currentDir = Directory.GetCurrentDirectory();

            TargetDirPath = Path.Combine(currentDir, args[0]);
            if (!Directory.Exists(TargetDirPath))
            {
                Console.Error.WriteLine($"Unable to locate directory {TargetDirPath}");
                throw new Exception();
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
                throw new Exception();
            }
            return;
        }
    }
}
