using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFollowUp.API.DTOs;
using SmartFollowUp.API.Services;
using System.Security.Claims;

namespace SmartFollowUp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(result);
        }
        // POST api/auth/register-patient
        [HttpPost("register-patient")]
        public async Task<IActionResult> RegisterPatient(RegisterPatientRequestDto request)
        {
            var result = await _authService.RegisterPatientAsync(request);
            if (result == null)
                return BadRequest(new { message = "Email already exists" });

            return Ok(result);
        }
        // POST api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto request)
        {
            var success = await _authService.ForgotPasswordAsync(request.Email);
            if (!success)
                return NotFound(new { message = "Email not found" });

            return Ok(new { message = "Reset token sent to your email" });
        }

        // POST api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto request)
        {
            var success = await _authService.ResetPasswordAsync(request);
            if (!success)
                return BadRequest(new { message = "Invalid token or email" });

            return Ok(new { message = "Password reset successfully" });
        }

        // POST api/auth/request-doctor
        [HttpPost("request-doctor")]
        public async Task<IActionResult> RequestDoctor(DoctorRequestDto request)
        {
            var success = await _authService.SubmitDoctorRequestAsync(request);
            if (!success)
                return BadRequest(new { message = "Email already exists" });

            return Ok(new { message = "Request submitted successfully, pending admin approval" });
        }

        // POST api/auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (result == null)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            return Ok(result);
        }

        // POST api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _authService.LogoutAsync(userId);
            return Ok(new { message = "Logged out successfully" });
        }

        // POST api/auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto request)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var success = await _authService.ChangePasswordAsync(userId, request);
            if (!success)
                return BadRequest(new { message = "Current password is incorrect" });

            return Ok(new { message = "Password changed successfully" });
        }

    }
}