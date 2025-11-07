using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Harvest.Models;

namespace Harvest.Services;

public class SessionService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly AuthService _authService;
    private const string UserSessionKey = "CurrentUser";

    public SessionService(ProtectedSessionStorage sessionStorage, AuthService authService)
    {
        _sessionStorage = sessionStorage;
        _authService = authService;
    }

    /// <summary>
    /// Get current logged in user
    /// </summary>
    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<int>(UserSessionKey);
            if (result.Success && result.Value > 0)
            {
                return await _authService.GetUserByIdAsync(result.Value);
            }
        }
        catch
        {
            // Session storage might not be available in prerender mode
        }
        return null;
    }

    /// <summary>
    /// Set current user session
    /// </summary>
    public async Task SetCurrentUserAsync(User user)
    {
        await _sessionStorage.SetAsync(UserSessionKey, user.UserId);
    }

    /// <summary>
    /// Clear current user session (logout)
    /// </summary>
    public async Task ClearCurrentUserAsync()
    {
        await _sessionStorage.DeleteAsync(UserSessionKey);
    }

    /// <summary>
    /// Check if user is logged in
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        var user = await GetCurrentUserAsync();
        return user != null;
    }

    /// <summary>
    /// Check if user has specific role
    /// </summary>
    public async Task<bool> HasRoleAsync(string role)
    {
        var user = await GetCurrentUserAsync();
        return user?.Role == role;
    }

    /// <summary>
    /// Check if user is customer
    /// </summary>
    public async Task<bool> IsCustomerAsync()
    {
        return await HasRoleAsync("Customer");
    }

    /// <summary>
    /// Check if user is admin
    /// </summary>
    public async Task<bool> IsAdminAsync()
    {
        return await HasRoleAsync("Admin");
    }

    /// <summary>
    /// Get current customer ID
    /// </summary>
    public async Task<int?> GetCurrentCustomerIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.Customer?.CustomerId;
    }
}
