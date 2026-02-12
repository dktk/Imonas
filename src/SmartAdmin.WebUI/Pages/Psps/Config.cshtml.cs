using Application.Features.Psps.Commands;
using Application.Features.Psps.Queries;

namespace SmartAdmin.WebUI.Pages.Psps
{
    [Authorize(Roles = "Admin,System Admin,Config Manager")]
    public class ConfigModel(
        IStringLocalizer<ConfigModel> localizer,
        ISender mediator) : PageModel
    {
        public PspConfigurationDto? PspConfig { get; set; }
        public Dictionary<string, string> ConfigValues { get; set; } = new();

        // Known PSP config structures
        public static readonly Dictionary<string, string[]> PspConfigFields = new(StringComparer.OrdinalIgnoreCase)
        {
            ["UNIWIRE"] = ["ApiKey", "ApiSecret", "BaseUrl"],
            ["NODAPAY"] = ["ApiKey", "SignKey"],
            ["NUMMUSPAY"] = ["ApiKey", "ProjectIds"],
            ["RASTPAY"] = ["BaseUrl", "AuthToken", "SecretKeys"],
            ["PAYSAGE"] = ["ReportUrl", "ApiVersion", "Secrets"]
        };

        public static readonly Dictionary<string, string> FieldLabels = new()
        {
            ["ApiKey"] = "API Key",
            ["ApiSecret"] = "API Secret",
            ["BaseUrl"] = "Base URL",
            ["SignKey"] = "Sign Key",
            ["ProjectIds"] = "Project IDs",
            ["AuthToken"] = "Auth Token",
            ["SecretKeys"] = "Secret Keys",
            ["ReportUrl"] = "Report URL",
            ["ApiVersion"] = "API Version",
            ["Secrets"] = "Shop Secrets"
        };

        public static readonly HashSet<string> ArrayFields = new()
        {
            "ProjectIds", "SecretKeys"
        };

        public static readonly HashSet<string> ObjectArrayFields = new()
        {
            "Secrets"
        };

        public static readonly HashSet<string> SensitiveFields = new()
        {
            "ApiKey", "ApiSecret", "SignKey", "AuthToken"
        };

        public bool HasApiConfig => PspConfig != null && PspConfigFields.ContainsKey(PspConfig.PspCode);

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PspConfig = await mediator.Send(new GetPspConfigurationQuery { PspId = id });

            if (PspConfig == null)
                return RedirectToPage("/Psps/Admin");

            ParseConfigJson();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync(int id, string configJson)
        {
            if (string.IsNullOrWhiteSpace(configJson))
            {
                return new CamelCaseJsonResult(new { succeeded = false, message = localizer["Configuration data is required."].Value });
            }

            var result = await mediator.Send(new SavePspConfigurationCommand
            {
                PspId = id,
                ConfigJson = configJson
            });

            if (result.Success)
            {
                return new CamelCaseJsonResult(new
                {
                    succeeded = true,
                    message = localizer["Configuration saved successfully."].Value
                });
            }

            return new CamelCaseJsonResult(new { succeeded = false, message = result.Message ?? "Failed to save configuration." });
        }

        private void ParseConfigJson()
        {
            if (PspConfig == null || string.IsNullOrWhiteSpace(PspConfig.ConfigJson))
                return;

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(PspConfig.ConfigJson);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    ConfigValues[prop.Name] = prop.Value.ToString();
                }
            }
            catch
            {
                // Invalid JSON — start fresh
            }
        }
    }
}
