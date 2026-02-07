using Application.Features.Users.DTOs;
using SG.Common;

namespace Application.Common.Interfaces.Identity;

public interface IUserService : IService
{
    Task<IEnumerable<UserDto>> GetUsersAsync(string? searchTerm = null, string? roleFilter = null, string? statusFilter = null);
    Task<UserDetailsDto?> GetUserByIdAsync(string userId);
    Task<UserStatsDto> GetUserStatsAsync();
    Task<IEnumerable<RoleDto>> GetRolesAsync();
    Task<PermissionMatrixDto> GetPermissionMatrixAsync();
    Task<Result<UserDto>> CreateUserAsync(string email, string displayName, string password, List<string> roles, bool isActive, bool emailConfirmed);
    Task<Result<bool>> UpdateUserAsync(string userId, string displayName, List<string> roles, bool isActive);
    Task<Result<bool>> ToggleUserStatusAsync(string userId);
    Task<Result<string>> ResetPasswordAsync(string userId);
    Task<Result<bool>> CreateRoleAsync(string name, string? description, List<string>? permissions = null);
    Task<Result<bool>> UpdateRolePermissionsAsync(string roleName, List<string> permissions);
    Task<Result<int>> SeedRolesAsync();

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    Task<bool> IsInRoleAsync(string userId, string roleName);

    /// <summary>
    /// Gets the first available admin user ID. Returns null if no admin exists.
    /// </summary>
    Task<string?> GetFirstAdminUserIdAsync();

    /// <summary>
    /// Determines who should be assigned a case based on the current user's role.
    /// If the user is an Admin or System Admin, returns their ID.
    /// Otherwise, returns the first available Admin user ID.
    /// </summary>
    Task<string?> GetCaseAssigneeAsync(string? currentUserId);
}
