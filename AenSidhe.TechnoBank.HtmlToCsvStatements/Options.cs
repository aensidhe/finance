using CommandLine;

namespace AenSidhe.TechnoBank.HtmlToCsvStatements
{
    internal class Options
    {
        [Option('s', "File with statements from it.tb.by.", Required = true)]
        public string StatementFile { get; set; }

        [Option('o', "Output csv file. Encoding will be UTF-8.", DefaultValue = "output_yyyy_MM_dd_HH_mm_ss.csv")]
        public string OutputFile { get; set; }

        [Option('c', "Main currency of transactions", Required = true)]
        public string MainCurrency { get; set; }

        [Option('g', "If true, tool will generate two transactions for transaction in currencies others than main.", DefaultValue = true)]
        public bool GenerateExchangeTransaction { get; set; }
    }
}