using Domain;

namespace PspConnectors
{
    public class TargetService
    {
        public async Task<string> Save(string filePath, TargetData[] targetData)
        {
            if (!File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, targetData.ToJson());
            }

            return filePath;
        }
    }
}
