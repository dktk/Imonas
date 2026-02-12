using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace SmartAdmin.WebUI.Pages.Api
{
    public class CsvHeadersModel(
        IApplicationDbContext context) : PageModel
    {
        public IActionResult OnGet()
        {
            return NotFound();
        }

        /// <summary>
        /// Extract headers from an uploaded CSV file
        /// </summary>
        public async Task<IActionResult> OnPostUploadAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var firstLine = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(firstLine))
                    return BadRequest(new { error = "CSV file is empty" });

                var headers = ParseCsvLine(firstLine);
                return new CamelCaseJsonResult(new { headers, fileName = file.FileName });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Failed to parse CSV: {ex.Message}" });
            }
        }

        /// <summary>
        /// Extract headers from an existing uploaded file (RawPayment)
        /// </summary>
        public async Task<IActionResult> OnGetFromFileAsync(int fileId)
        {
            var rawPayment = await context.RawPayments
                .Where(r => r.Id == fileId)
                .Select(r => new { r.RawContent, r.FileName })
                .FirstOrDefaultAsync();

            if (rawPayment == null)
                return NotFound(new { error = "File not found" });

            try
            {
                var content = Encoding.UTF8.GetString(rawPayment.RawContent);
                var firstLine = content.Split('\n').FirstOrDefault();

                if (string.IsNullOrWhiteSpace(firstLine))
                    return BadRequest(new { error = "CSV file is empty" });

                var headers = ParseCsvLine(firstLine.TrimEnd('\r'));
                return new CamelCaseJsonResult(new { headers, fileName = rawPayment.FileName });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Failed to parse CSV: {ex.Message}" });
            }
        }

        private static string[] ParseCsvLine(string line)
        {
            var headers = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    headers.Add(current.ToString().Trim().Trim('"'));
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            headers.Add(current.ToString().Trim().Trim('"'));
            return headers.ToArray();
        }
    }
}
