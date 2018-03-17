using CommandLine;
using System.Threading;

namespace BoundlessMsgPackToJson
{
    class Program
    {
        class Options
        {
            [Option('i', "input", Required = true, HelpText = "Specifies the input directory or MsgPack file to read.")]
            public string InputFile { get; set; }

            [Option('o', "output", Required = true, HelpText = "Specifies the output directory to write json file(s).")]
            public string OutputDir { get; set; }
        }

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args).
                WithParsed(cur => Parser.DoParse(cur.OutputDir, cur.InputFile));

            if (System.Diagnostics.Debugger.IsAttached)
            {
                using (ManualResetEvent waitForever = new ManualResetEvent(false))
                {
                    waitForever.WaitOne();
                }
            }
        }
    }
}
