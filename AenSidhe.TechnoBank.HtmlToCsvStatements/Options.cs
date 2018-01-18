using CommandLine;

namespace AenSidhe.TechnoBank.HtmlToCsvStatements
{
    internal class Options
    {
        [Option('s', HelpText = "File with statements from it.tb.by.", Required = true)]
        public string StatementFile { get; set; }

        [Option('o', HelpText = "Output csv file. Encoding will be UTF-8.")]
        public string OutputFile { get; set; } = "output_yyyy_MM_dd_HH_mm_ss.csv";

        [Option('c', HelpText = "Main currency of transactions", Required = true)]
        public string MainCurrency { get; set; }

        [Option('g', HelpText = "If true, tool will generate two transactions for transaction in currencies others than main.")]
        public bool GenerateExchangeTransaction { get; set; } = true;
    }
}