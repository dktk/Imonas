namespace Domain.Constants
{
    public static class Roles
    {
        public const string SystemAdmin = "System Admin";
        public const string Admin = "Admin";
        public const string Analyst = "Analyst";
        public const string Viewer = "Viewer";
        public const string FinanceAdmin = "Finance Admin";
        public const string ConfigManager = "Config Manager";

        public static List<(string Name, string Description)> AllRoles => new()
        {
            (SystemAdmin, "Full access to all system features and settings"),
            (Admin, "Coordinate all roles except System Admin, full management access"),
            (Analyst, "View dashboard, start runs, manage cases, upload files"),
            (Viewer, "View-only access to dashboard and reports"),
            (FinanceAdmin, "Financial reconciliation and reporting access"),
            (ConfigManager, "Configuration and rules management access")
        };

        public static Dictionary<string, List<string>> DefaultRolePermissions => new()
        {
            {
                SystemAdmin, new List<string>
                {
                    Permissions.ViewDashboard,
                    Permissions.StartRuns,
                    Permissions.ManageCases,
                    Permissions.UploadFiles,
                    Permissions.ConfigureRules,
                    Permissions.SystemConfig,
                    Permissions.UserManagement
                }
            },
            {
                Admin, new List<string>
                {
                    Permissions.ViewDashboard,
                    Permissions.StartRuns,
                    Permissions.ManageCases,
                    Permissions.UploadFiles,
                    Permissions.ConfigureRules,
                    Permissions.SystemConfig,
                    Permissions.UserManagement
                }
            },
            {
                Analyst, new List<string>
                {
                    Permissions.ViewDashboard,
                    Permissions.StartRuns,
                    Permissions.ManageCases,
                    Permissions.UploadFiles
                }
            },
            {
                Viewer, new List<string>
                {
                    Permissions.ViewDashboard
                }
            },
            {
                FinanceAdmin, new List<string>
                {
                    Permissions.ViewDashboard,
                    Permissions.StartRuns,
                    Permissions.ManageCases,
                    Permissions.UploadFiles
                }
            },
            {
                ConfigManager, new List<string>
                {
                    Permissions.ViewDashboard,
                    Permissions.ConfigureRules,
                    Permissions.SystemConfig
                }
            }
        };
    }
}
