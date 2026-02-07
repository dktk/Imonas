using CsvHelper;

using Domain;

using PspConnectors.Methods;

using SG.Common;

using System.Globalization;
using System.Text;

namespace PspConnectors.Domain
{
    public class RunSummaryItem
    {
        public Guid RunId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long Duration { get; set; }
        
        // todo: take currency into account 

        public decimal TotalAmount { get; set; }

        public string PspName { get; set; }
        public string SourceName { get; set; }
        public string TargetName { get; set; }
        public int SourceSize { get; set; }
        public int TargetSize { get; set; }

        public List<InternalTransaction> UnmatchedSources { get; set; }
        public List<TargetData> UnmatchedTargets { get; set; }

        public string SourceService1Name { get; set; }
        public string SourceService2Name { get; set; }

        public static string Store(string path, RunSummaryItem item)
        {
            var content = new StringBuilder()
                                .AppendLine($"RunId: {item.RunId}")
                                .AppendLine($"PSP: {item.PspName}")
                                .AppendLine()
                                .AppendLine($"Start: {item.StartDate}")
                                .AppendLine($"End: {item.EndDate}")
                                .AppendLine()
                                .AppendLine($"Duration: {item.Duration}s")
                                
                                .AppendLine($"Amount: {item.TotalAmount}")
                                
                                .AppendLine($"Matched Source: {item.SourceName} {item.SourceSize - item.UnmatchedSources.Count}/{item.SourceSize}")
                                .AppendLine($"Matched Target: {item.TargetName} {item.TargetSize - item.UnmatchedTargets.Count}/{item.TargetSize}")

                                .ToString();

            File.WriteAllText(path, content);

            WriteUnmatchedItems(path, $"Txs - available in {item.SourceName}, but not in {item.TargetName}", item.UnmatchedSources);
            WriteUnmatchedItems(path, $"Txs - available in {item.TargetName}, but not in {item.SourceName}", item.UnmatchedTargets);

            return content;
        }

        private static void WriteUnmatchedItems<T>(string path, string sectionName, List<T> items)
        {
            var content = new StringBuilder()
                                    .AppendLine(sectionName)
                                    .AppendLine()
                                    .ToString();

            using (var writer = new StreamWriter(path, true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                writer.Write(Environment.NewLine + sectionName + Environment.NewLine);
                if (items.Count > 0)
                {
                    csv.WriteRecords(items);
                }
                else
                {
                    writer.WriteLine("No unmatched transactions.");
                }
            }
        }
    }
}
