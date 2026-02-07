namespace Domain.Constants
{
    public static class Permissions
    {
        public const string ViewDashboard = "Permissions.Dashboard.View";
        public const string StartRuns = "Permissions.Runs.Start";
        public const string ManageCases = "Permissions.Cases.Manage";
        public const string UploadFiles = "Permissions.Files.Upload";
        public const string ConfigureRules = "Permissions.Rules.Configure";
        public const string SystemConfig = "Permissions.System.Config";
        public const string UserManagement = "Permissions.Users.Manage";

        public static List<string> AllPermissions => new()
        {
            ViewDashboard,
            StartRuns,
            ManageCases,
            UploadFiles,
            ConfigureRules,
            SystemConfig,
            UserManagement
        };

        public static Dictionary<string, string> PermissionDescriptions => new()
        {
            { ViewDashboard, "View Dashboard" },
            { StartRuns, "Start Reconciliation Runs" },
            { ManageCases, "Manage Exception Cases" },
            { UploadFiles, "Upload Files" },
            { ConfigureRules, "Configure Matching Rules" },
            { SystemConfig, "System Configuration" },
            { UserManagement, "User Management" }
        };
    }
}
