using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace PspConnectors.Services
{
    public class CsvService
    {
        public static List<TTransaction> ReadCsv<TTransaction>(string filePath, Action<CsvContext> configurator)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = SG.Common.Constants.Comma
            });

            configurator(csv.Context);

            return new List<TTransaction>(csv.GetRecords<TTransaction>());
        }

        public static List<TTransaction> ReadCsv<TTransaction>(byte[] content, Action<CsvContext> configurator)
        {
            try
            {
                using var memoryStream = new MemoryStream(content);
                using var reader = new StreamReader(memoryStream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = SG.Common.Constants.Comma
                });

                configurator(csv.Context);

                return new List<TTransaction>(csv.GetRecords<TTransaction>());
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
