using CommandLine;

namespace HtmlToCsvStatements
{
    internal class Options
    {
        [Option('s', "statement", Required = true)]
        public string StatementFile { get; set; }

        [Option('o', "output", DefaultValue = "output_yyyy_MM_dd_HH_mm_ss.csv")]
        public string OutputFile { get; set; }

        [Option('c', "main-currency", Required = true)]
        public string MainCurrency { get; set; }
    }
}