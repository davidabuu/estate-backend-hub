using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaymentService.Application.Interface;
using PaymentService.Application.Services;
using PaymentService.Infrastructure.Data;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Configuration
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ==========================================
// 2. Database Context
// ==========================================
builder.Services.AddDbContext<PaymentDbContext>(options =>
	options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// ==========================================
// 3. Services
// ==========================================
builder.Services.AddScoped<IPaystackService, PaystackService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();

// ==========================================
// 4. MediatR (CQRS)
// ==========================================
builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// ==========================================
// 5. JWT Authentication
// ==========================================
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtSecretKey))
{
	throw new InvalidOperationException(
		"Jwt:SecretKey is missing. Set it via appsettings, environment variable (Jwt__SecretKey), or Render secret.");
}

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

// ==========================================
// 6. Authorization
// ==========================================
builder.Services.AddAuthorization();

// ==========================================
// 7. Controllers
// ==========================================
builder.Services.AddControllers();

// ==========================================
// 8. Swagger with JWT Support
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "PaymentService API",
		Version = "v1",
		Description = "EstateHub Payment Service - Paystack Integration"
	});

	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Enter your JWT token below (no need to type 'Bearer ')."
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
// 9. CORS
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
// 10. Background Service for Idempotency Cleanup
// ==========================================
builder.Services.AddHostedService<IdempotencyCleanupService>();

// ==========================================
// 11. HTTP Client for Paystack
// ==========================================
builder.Services.AddHttpClient<IPaystackService, PaystackService>(client =>
{
	client.BaseAddress = new Uri(builder.Configuration["Paystack:BaseUrl"] ?? "https://api.paystack.co");
	client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["Paystack:SecretKey"]}");
});

// ==========================================
// 12. Health Checks
// ==========================================


// ==========================================
// Build App
// ==========================================
var app = builder.Build();

// ==========================================
// Middleware Pipeline
// ==========================================

// ✅ ENABLE SWAGGER FOR ALL ENVIRONMENTS!
app.UseSwagger();
app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentService API v1");
	options.RoutePrefix = "swagger";
});

// ✅ Root endpoint redirects to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Health Che

// Controllers
app.MapControllers();

// ==========================================
// Run Migrations on Startup
// ==========================================
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
	Console.WriteLine("📦 Applying database migrations...");
	await dbContext.Database.MigrateAsync();
	Console.WriteLine("✅ Database migrations applied successfully!");
}

// ==========================================
// Run App
// ==========================================
Console.WriteLine("✅ PaymentService is ready!");
app.Run();