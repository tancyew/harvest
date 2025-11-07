using Microsoft.EntityFrameworkCore;
using Harvest.Data;
using Harvest.Models;

namespace Harvest.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Request OTP for login via email or phone
    /// </summary>
    public async Task<(bool Success, string Message)> RequestOtpAsync(string emailOrPhone)
    {
        // Find user by email or phone
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == emailOrPhone || u.Phone == emailOrPhone);

        if (user == null)
        {
            return (false, "User not found. Please contact administrator to create an account.");
        }

        if (!user.IsActive)
        {
            return (false, "Your account is inactive. Please contact administrator.");
        }

        // Check if dummy mode is enabled
        var dummyMode = _configuration.GetValue<bool>("OtpSettings:DummyMode", true);
        var otpCode = dummyMode
            ? _configuration.GetValue<string>("OtpSettings:DummyOtpCode", "123456")
            : GenerateRandomOtp();

        var expiryMinutes = _configuration.GetValue<int>("OtpSettings:OtpExpiryMinutes", 5);

        // Invalidate any existing unused OTPs for this user
        var existingOtps = await _context.OtpCodes
            .Where(o => o.UserId == user.UserId && !o.IsUsed)
            .ToListAsync();

        foreach (var otp in existingOtps)
        {
            otp.IsUsed = true;
            otp.UsedDate = DateTime.UtcNow;
        }

        // Create new OTP
        var newOtp = new OtpCode
        {
            UserId = user.UserId,
            Code = otpCode,
            ExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes),
            IsUsed = false,
            CreatedDate = DateTime.UtcNow
        };

        _context.OtpCodes.Add(newOtp);
        await _context.SaveChangesAsync();

        if (dummyMode)
        {
            return (true, $"OTP sent successfully! (Dummy Mode: Use code {otpCode})");
        }
        else
        {
            // TODO: In production, send OTP via SMS/Email
            // await _emailService.SendOtpEmail(user.Email, otpCode);
            // await _smsService.SendOtpSms(user.Phone, otpCode);
            return (true, "OTP sent successfully to your registered contact.");
        }
    }

    /// <summary>
    /// Verify OTP and authenticate user
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> VerifyOtpAsync(string emailOrPhone, string otpCode)
    {
        // Find user by email or phone
        var user = await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Email == emailOrPhone || u.Phone == emailOrPhone);

        if (user == null)
        {
            return (false, "User not found.", null);
        }

        // Find valid OTP
        var otp = await _context.OtpCodes
            .Where(o => o.UserId == user.UserId
                     && o.Code == otpCode
                     && !o.IsUsed
                     && o.ExpiryTime > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedDate)
            .FirstOrDefaultAsync();

        if (otp == null)
        {
            return (false, "Invalid or expired OTP code.", null);
        }

        // Mark OTP as used
        otp.IsUsed = true;
        otp.UsedDate = DateTime.UtcNow;

        // Update user's last login date
        user.LastLoginDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, "Login successful!", user);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    /// <summary>
    /// Get user by email or phone
    /// </summary>
    public async Task<User?> GetUserByEmailOrPhoneAsync(string emailOrPhone)
    {
        return await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Email == emailOrPhone || u.Phone == emailOrPhone);
    }

    /// <summary>
    /// Register a new customer user
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> RegisterCustomerAsync(
        string email, string phone, string fullName, string businessName, string? address = null)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email || u.Phone == phone);

        if (existingUser != null)
        {
            return (false, "User with this email or phone already exists.", null);
        }

        // Create new user
        var user = new User
        {
            Email = email,
            Phone = phone,
            FullName = fullName,
            Role = "Customer",
            UserType = "Customer",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create customer profile
        var customer = new Customer
        {
            UserId = user.UserId,
            BusinessName = businessName,
            Address = address,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Reload user with customer
        user = await GetUserByIdAsync(user.UserId);

        return (true, "Registration successful!", user);
    }

    /// <summary>
    /// Generate a random 6-digit OTP
    /// </summary>
    private string GenerateRandomOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    /// <summary>
    /// Check if user exists
    /// </summary>
    public async Task<bool> UserExistsAsync(string emailOrPhone)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == emailOrPhone || u.Phone == emailOrPhone);
    }
}
