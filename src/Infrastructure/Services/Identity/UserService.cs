using Application.Common.Interfaces.Identity;
using Application.Features.Users.DTOs;
using Domain.Constants;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SG.Common;
using System.Security.Claims;

namespace Infrastructure.Services.Identity;

public class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : IUserService
{
    public async Task<IEnumerable<UserDto>> GetUsersAsync(string? searchTerm = null, string? roleFilter = null, string? statusFilter = null)
    {
        var query = userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchTerm) ||
                u.DisplayName.ToLower().Contains(searchTerm) ||
                u.UserName.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            query = statusFilter.ToLower() switch
            {
                "active" => query.Where(u => u.IsActive),
                "inactive" => query.Where(u => !u.IsActive),
                _ => query
            };
        }

        var users = await query.ToListAsync();
        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            if (!string.IsNullOrEmpty(roleFilter) && !roles.Contains(roleFilter))
            {
                continue;
            }

            result.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName ?? string.Empty,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            });
        }

        return result;
    }

    public async Task<UserDetailsDto?> GetUserByIdAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await userManager.GetRolesAsync(user);

        return new UserDetailsDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName ?? string.Empty,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LockoutEnd = user.LockoutEnd,
            Roles = roles.ToList()
        };
    }

    public async Task<UserStatsDto> GetUserStatsAsync()
    {
        var users = await userManager.Users.ToListAsync();

        var stats = new UserStatsDto
        {
            TotalUsers = users.Count,
            ActiveUsers = users.Count(u => u.IsActive),
            InactiveUsers = users.Count(u => !u.IsActive),
            LockedOutUsers = users.Count(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow)
        };

        var roles = await roleManager.Roles.ToListAsync();
        foreach (var role in roles)
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);
            stats.UsersByRole[role.Name ?? string.Empty] = usersInRole.Count;
        }

        return stats;
    }

    public async Task<IEnumerable<RoleDto>> GetRolesAsync()
    {
        var roles = await roleManager.Roles.ToListAsync();
        var result = new List<RoleDto>();

        foreach (var role in roles)
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);
            result.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                UserCount = usersInRole.Count
            });
        }

        return result;
    }

    public async Task<PermissionMatrixDto> GetPermissionMatrixAsync()
    {
        var result = new PermissionMatrixDto
        {
            Permissions = Permissions.AllPermissions
        };

        var roles = await roleManager.Roles
            .Where(r => r.Name != Roles.SystemAdmin)
            .ToListAsync();

        foreach (var role in roles)
        {
            var claims = await roleManager.GetClaimsAsync(role);
            var rolePermissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

            var rolePermDto = new RolePermissionsDto
            {
                RoleName = role.Name ?? string.Empty
            };

            foreach (var permission in Permissions.AllPermissions)
            {
                rolePermDto.Permissions[permission] = rolePermissions.Contains(permission);
            }

            result.RolePermissions.Add(rolePermDto);
        }

        return result;
    }

    public async Task<Result<UserDto>> CreateUserAsync(string email, string displayName, string password, List<string> roles, bool isActive, bool emailConfirmed)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return Result<UserDto>.BuildFailure("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            IsActive = isActive,
            EmailConfirmed = emailConfirmed,
            Site = "Default"
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Result<UserDto>.BuildFailure($"Failed to create user: {errors}");
        }

        if (roles.Any())
        {
            var roleResult = await userManager.AddToRolesAsync(user, roles);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Result<UserDto>.BuildFailure($"User created but failed to assign roles: {errors}");
            }
        }

        var userRoles = await userManager.GetRolesAsync(user);
        return Result<UserDto>.BuildSuccess(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            Roles = userRoles.ToList()
        }, "User created successfully.");
    }

    public async Task<Result<bool>> UpdateUserAsync(string userId, string displayName, List<string> roles, bool isActive)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result<bool>.BuildFailure("User not found.");
        }

        user.DisplayName = displayName;
        user.IsActive = isActive;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return Result<bool>.BuildFailure($"Failed to update user: {errors}");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(roles).ToList();
        var rolesToAdd = roles.Except(currentRoles).ToList();

        if (rolesToRemove.Any())
        {
            await userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }

        if (rolesToAdd.Any())
        {
            await userManager.AddToRolesAsync(user, rolesToAdd);
        }

        return Result<bool>.BuildSuccess(true, "User updated successfully.");
    }

    public async Task<Result<bool>> ToggleUserStatusAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result<bool>.BuildFailure("User not found.");
        }

        user.IsActive = !user.IsActive;
        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return Result<bool>.BuildFailure($"Failed to toggle user status: {errors}");
        }

        var status = user.IsActive ? "activated" : "deactivated";
        return Result<bool>.BuildSuccess(true, $"User {status} successfully.");
    }

    public async Task<Result<string>> ResetPasswordAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result<string>.BuildFailure("User not found.");
        }

        var newPassword = GenerateTemporaryPassword();
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<string>.BuildFailure($"Failed to reset password: {errors}");
        }

        return Result<string>.BuildSuccess(newPassword, "Password reset successfully.");
    }

    public async Task<Result<bool>> CreateRoleAsync(string name, string? description, List<string>? permissions = null)
    {
        var existingRole = await roleManager.FindByNameAsync(name);
        if (existingRole != null)
        {
            return Result<bool>.BuildFailure("A role with this name already exists.");
        }

        var role = new ApplicationRole(name, description);
        var createResult = await roleManager.CreateAsync(role);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Result<bool>.BuildFailure($"Failed to create role: {errors}");
        }

        if (permissions != null && permissions.Any())
        {
            foreach (var permission in permissions)
            {
                await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            }
        }

        return Result<bool>.BuildSuccess(true, "Role created successfully.");
    }

    public async Task<Result<bool>> UpdateRolePermissionsAsync(string roleName, List<string> permissions)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            return Result<bool>.BuildFailure("Role not found.");
        }

        var existingClaims = await roleManager.GetClaimsAsync(role);
        var permissionClaims = existingClaims.Where(c => c.Type == "Permission").ToList();

        foreach (var claim in permissionClaims)
        {
            await roleManager.RemoveClaimAsync(role, claim);
        }

        foreach (var permission in permissions)
        {
            await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
        }

        return Result<bool>.BuildSuccess(true, "Role permissions updated successfully.");
    }

    public async Task<Result<int>> SeedRolesAsync()
    {
        var createdCount = 0;

        foreach (var (name, description) in Roles.AllRoles)
        {
            var existingRole = await roleManager.FindByNameAsync(name);
            if (existingRole != null)
            {
                continue;
            }

            var role = new ApplicationRole(name, description);
            var result = await roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                createdCount++;

                if (Roles.DefaultRolePermissions.TryGetValue(name, out var rolePermissions))
                {
                    foreach (var permission in rolePermissions)
                    {
                        await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                    }
                }
            }
        }

        return Result<int>.BuildSuccess(createdCount, $"Seeded {createdCount} new role(s).");
    }

    public async Task<bool> IsInRoleAsync(string userId, string roleName)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        return await userManager.IsInRoleAsync(user, roleName);
    }

    public async Task<string?> GetFirstAdminUserIdAsync()
    {
        // Try to find a user with Admin role first
        var adminUsers = await userManager.GetUsersInRoleAsync(Roles.Admin);
        var activeAdmin = adminUsers.FirstOrDefault(u => u.IsActive);

        if (activeAdmin != null)
        {
            return activeAdmin.Id;
        }

        // Fall back to System Admin if no regular Admin is found
        var systemAdminUsers = await userManager.GetUsersInRoleAsync(Roles.SystemAdmin);
        var activeSystemAdmin = systemAdminUsers.FirstOrDefault(u => u.IsActive);

        return activeSystemAdmin?.Id;
    }

    public async Task<string?> GetCaseAssigneeAsync(string? currentUserId)
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            // No current user, assign to first available admin
            return await GetFirstAdminUserIdAsync();
        }

        // Check if current user is Admin or System Admin
        var isAdmin = await IsInRoleAsync(currentUserId, Roles.Admin);
        var isSystemAdmin = await IsInRoleAsync(currentUserId, Roles.SystemAdmin);

        if (isAdmin || isSystemAdmin)
        {
            // User is an admin, assign to themselves
            return currentUserId;
        }

        // User is not an admin, find an admin to assign to
        return await GetFirstAdminUserIdAsync();
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
