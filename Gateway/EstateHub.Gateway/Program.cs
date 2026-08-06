using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Configuration
// ==========================================
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtSecretKey))
{
	throw new InvalidOperationException(
		"Jwt:SecretKey is missing. Set it via appsettings, environment variable (Jwt__SecretKey), or Render secret.");
}

// ==========================================
// 2. JWT Authentication
// ==========================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = jwtIssuer,

			ValidateAudience = true,
			ValidAudience = jwtAudience,

			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(jwtSecretKey)
			),

			ValidateLifetime = true,
			ClockSkew = TimeSpan.FromMinutes(1)
		};
	});

builder.Services.AddAuthorization();

// ==========================================
// 3. CORS (Allow Frontend)
// ==========================================
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll",
		builder => builder
			.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader());
});

// ==========================================
// 4. Rate Limiting (Simple Fixed Window)
// ==========================================
builder.Services.AddRateLimiter(options =>
{
	options.AddFixedWindowLimiter("default", opt =>
	{
		opt.PermitLimit = 100;              // 100 requests per window
		opt.Window = TimeSpan.FromMinutes(1); // 1 minute window
		opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		opt.QueueLimit = 0;                 // No queue, immediately reject
	});
});

// ==========================================
// 5. YARP Reverse Proxy
// ==========================================
builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ==========================================
// 6. Health Checks
// ==========================================
builder.Services.AddHealthChecks()
	.AddUrlGroup(
		new Uri("https://estate-backend-hub-qq4q.onrender.com/health"),
		"UserService",
		Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
		new[] { "services" })
	.AddUrlGroup(
		new Uri("https://estate-backend-hub-2.onrender.com/health"),
		"PaymentService",
		Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
		new[] { "services" });

// ==========================================
// 7. Swagger
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "EstateHub API Gateway",
		Version = "v1",
		Description = "YARP Gateway for EstateHub Microservices"
	});

	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Enter your JWT token below."
	});

	options.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});

// ==========================================
// Build App
// ==========================================
var app = builder.Build();

// ==========================================
// Middleware Pipeline
// ==========================================

// 1. Swagger
app.UseSwagger();
app.UseSwaggerUI();

// 2. Root Redirect
app.MapGet("/", () => Results.Redirect("/swagger"));

// 3. CORS
app.UseCors("AllowAll");

// 4. Rate Limiting
app.UseRateLimiter();

// 5. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Health Checks
app.MapHealthChecks("/health");

// 7. Reverse Proxy (YARP)
app.MapReverseProxy();

// ==========================================
// Run App
// ==========================================
app.Run();