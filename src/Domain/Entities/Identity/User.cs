namespace Domain.Entities.Identity;

/// <summary>
/// Domain representation of an application user.
/// Maps to the AspNetUsers table for read operations.
/// </summary>
public class User
{
    public string Id { get; set; } = default!;
    public string? UserName { get; set; }
    public string? NormalizedUserName { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }

    // Custom properties
    public string? DisplayName { get; set; }
    public string? Site { get; set; }
    public string? ProfilePictureDataUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsLive { get; set; }
}
