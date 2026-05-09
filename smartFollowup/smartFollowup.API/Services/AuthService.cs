using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartFollowUp.API.Data;
using SmartFollowUp.API.DTOs;
using SmartFollowUp.API.Enums;
using SmartFollowUp.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartFollowUp.API.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public AuthService(AppDbContext context, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        // Login
        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = GenerateToken(user),
                RefreshToken = refreshToken,
                RefreshTokenExpiry = user.RefreshTokenExpiry.Value
            };
        }

        // Register Patient
        public async Task<AuthResponseDto?> RegisterPatientAsync(RegisterPatientRequestDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return null;

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Patient,
                Phone = request.Phone,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var patientProfile = new PatientProfile
            {
                UserId = user.Id,
                Age = request.Age,
                Gender = request.Gender == "male" ? Gender.Male : Gender.Female,
                ChronicDiseases = request.ChronicDiseases,
                Allergies = request.Allergies,
                CurrentMedications = request.CurrentMedications
            };

            _context.PatientProfiles.Add(patientProfile);
            await _context.SaveChangesAsync();

            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = GenerateToken(user),
                RefreshToken = refreshToken,
                RefreshTokenExpiry = user.RefreshTokenExpiry.Value
            };
        }

        // Submit Doctor Request
        public async Task<bool> SubmitDoctorRequestAsync(DoctorRequestDto request)
        {
            var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email) ||
                              await _context.DoctorRequests.AnyAsync(r => r.Email == request.Email);

            if (emailExists) return false;

            var doctorRequest = new DoctorRequest
            {
                Name = request.Name,
                Email = request.Email,
                Specialty = request.Specialty,
                LicenseNumber = request.LicenseNumber,
                Status = DoctorRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.DoctorRequests.Add(doctorRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        // Forgot Password
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return false;

            var otpCode = new Random().Next(100000, 999999).ToString();
            user.OtpCode = otpCode;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                user.Name,
                otpCode
            );

            return true;
        }

        // Reset Password
        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == request.Email &&
                    u.OtpCode == request.Token &&
                    u.OtpExpiry > DateTime.UtcNow);

            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.OtpCode = null;
            user.OtpExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        // Logout
        public async Task<bool> LogoutAsync(long userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();

            return true;
        }

        // Change Password
        public async Task<bool> ChangePasswordAsync(long userId, ChangePasswordRequestDto request)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        // Generate Refresh Token
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        // Refresh Token
        public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.RefreshToken == refreshToken &&
                    u.RefreshTokenExpiry > DateTime.UtcNow);

            if (user == null) return null;

            var newAccessToken = GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiry = user.RefreshTokenExpiry.Value
            };
        }

        // Generate JWT Token
        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}