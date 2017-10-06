using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Serilog;

namespace HtmlToCsvStatements
{
    internal class Transaction
    {
        private static readonly Regex MultiSpace = new Regex("\\s{2,}", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex SingleSpace = new Regex("\\s", RegexOptions.Compiled | RegexOptions.Multiline);

        public static readonly SortedSet<string> Currencies = new SortedSet<string>();

        public static IEnumerable<Transaction> Parse(IReadOnlyList<HtmlNode> nodes, string mainCurrency)
        {
            var date = E(nodes[0]);
            var comment = E(nodes[2]);

            var sums = nodes[3].ChildNodes
                .Where(x => x.Name == "div")
                .Select(E)
                .Select(x => (x.Substring(x.Length - 3), SingleSpace.Replace(x.Substring(0, x.Length - 5), string.Empty)))
                .ToDictionary(x => x.Item1, x => decimal.Parse(x.Item2));

            if (sums.Count > 2)
            {
                Log.Error("More than two currencies: {Date}, {Comment}, {Spending}", date, comment, S(sums));
                yield break;
            }

            if (sums.Count == 0)
            {
                Log.Error("No spending: {Date}, {Comment}, {Spending}", date, comment, S(sums));
                yield break;
            }

            Currencies.UnionWith(sums.Keys);

            if (sums.Count == 1)
            {
                if (sums.Single().Key != mainCurrency)
                {
                    Log.Warning("No main currency: {Date}, {Comment}, {Spending}", date, comment, S(sums));
                }

                yield return new Transaction(date, comment, sums);
                yield break;
            }

            if (!sums.TryGetValue(mainCurrency, out var mainSum))
            {
                Log.Error("No main currency in 2 currency transaction: {Date}, {Comment}, {Spending}", date, comment, S(sums));
                yield break;
            }

            var convertedSum = sums.Single(x => x.Key != mainCurrency);

            yield return new Transaction(date, $"Exchange for {comment}", new Dictionary<string, decimal>
            {
                { mainCurrency, mainSum },
                { convertedSum.Key, -convertedSum.Value }
            });

            yield return new Transaction(date, comment, new Dictionary<string, decimal>
            {
                { convertedSum.Key, convertedSum.Value }
            });

            string S(IDictionary<string, decimal> o) => string.Join(", ", o.Select(x => $"'{x.Value}' of '{x.Key}'"));
            string E(HtmlNode node) => MultiSpace.Replace(node.InnerText.Trim(), " ");
        }

        private Transaction(string date, string comment, IReadOnlyDictionary<string, decimal> spending)
        {
            Date = date;
            Comment = comment;
            Spending = spending;
        }

        public string Date { get; }

        public string Comment { get; }

        public IReadOnlyDictionary<string, decimal> Spending { get; }
    }
}