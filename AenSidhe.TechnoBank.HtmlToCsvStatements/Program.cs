using System;
using System.IO;
using System.Linq;
using CommandLine;
using HtmlAgilityPack;
using Serilog;

namespace AenSidhe.TechnoBank.HtmlToCsvStatements
{
    public static class Program
    {
        private static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Parser.Default
                    .ParseArguments<Options>(args)
                    .WithParsed(DoAllWork);

                return 0;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Unexpected error. Exit code 2.");
                return 2;
            }
        }

        private static void DoAllWork(Options options)
        {
            var html = new HtmlDocument();
            html.Load(options.StatementFile);
            var divWithGrid = html.GetElementbyId("mainForm:tabBlock:cardAccountStatement:cardStatement");
            var grid = divWithGrid.SelectSingleNode("div/table/tbody");
            var transactions = grid.ChildNodes
                .Where(row => row.Name == "tr")
                .SelectMany(row => Transaction.Parse(row.ChildNodes.Where(x => x.Name == "td").ToArray(), options))
                .ToList();

            using (var file = new FileStream(options.OutputFile, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(file))
            using (var csv = new CsvHelper.Factory().CreateWriter(writer))
            {
                csv.WriteField("Date");
                csv.WriteField("Comment");
                foreach (var currency in Transaction.Currencies)
                {
                    csv.WriteField(currency);
                }
                csv.NextRecord();

                foreach (var transaction in transactions)
                {
                    csv.WriteField(transaction.Date);
                    csv.WriteField(transaction.Comment);
                    foreach (var currency in Transaction.Currencies)
                    {
                        csv.WriteField(transaction.Spending.TryGetValue(currency, out var amount) ? amount : 0m);
                    }
                    csv.NextRecord();
                }
            }

            Log.Information("Done");
        }
    }
}