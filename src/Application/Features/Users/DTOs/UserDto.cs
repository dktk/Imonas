namespace Application.Features.Users.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public DateTime? LastLogin { get; set; }
    }

    public class UserDetailsDto : UserDto
    {
        public string? ProfilePictureUrl { get; set; }
        public string? Site { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public IList<string> Permissions { get; set; } = new List<string>();
    }

    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UserCount { get; set; }
        public IList<string> Permissions { get; set; } = new List<string>();
    }

    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int LockedOutUsers { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new();
    }

    public class PermissionMatrixDto
    {
        public List<string> Permissions { get; set; } = new();
        public List<RolePermissionsDto> RolePermissions { get; set; } = new();
    }

    public class RolePermissionsDto
    {
        public string RoleName { get; set; } = string.Empty;
        public Dictionary<string, bool> Permissions { get; set; } = new();
    }
}
