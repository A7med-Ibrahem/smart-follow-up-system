using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartFollowUp.API.Data;
using SmartFollowUp.API.Services;
using System.Text;

namespace smartFollowup.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Services
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<CaseService>();
            builder.Services.AddScoped<ReportService>();
            builder.Services.AddScoped<PrescriptionService>();
            builder.Services.AddScoped<NoteService>();
            builder.Services.AddScoped<AlertService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<WoundImageService>();
            builder.Services.AddScoped<AdminService>();
            builder.Services.AddScoped<PatientService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<EscalationService>();

            // Hangfire
            builder.Services.AddHangfire(x => x.UseSqlServerStorage(
                builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddHangfireServer();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            builder.Services.AddControllers();
            builder.Services.AddAuthorization();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Hangfire
            app.UseHangfireDashboard("/hangfire");
            RecurringJob.AddOrUpdate<EscalationService>(
                "emergency-escalation",
                x => x.CheckAndEscalateAsync(),
                "*/30 * * * *");

            app.Run();
        }
    }
}