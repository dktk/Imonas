using Imonas.V9.Domain.Common;
using Imonas.V9.Domain.Enums;

namespace Imonas.V9.Domain.Entities.Auth;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? SsoProvider { get; set; }
    public string? SsoUserId { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
}

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Country { get; set; }

    public virtual User User { get; set; } = null!;
}

public class UserPermission : BaseEntity
{
    public Guid UserId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;

    public virtual User User { get; set; } = null!;
}
