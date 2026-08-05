using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
// 4. Services
// ==========================================
builder.Services.AddScoped<IPaystackService, PaystackService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();

// ==========================================
// 5. MediatR (CQRS)
// ==========================================
builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// ==========================================
// 6. JWT Authentication
// ==========================================
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
				Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
			)
		};
	});

// ==========================================
// 7. Authorization
// ==========================================
builder.Services.AddAuthorization();

// ==========================================
// 8. Controllers
// ==========================================
builder.Services.AddControllers();

// ==========================================
// 9. Swagger
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==========================================


// ==========================================
// 11. CORS
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
// 12. Background Service for Idempotency Cleanup
// ==========================================
builder.Services.AddHostedService<IdempotencyCleanupService>();

// ==========================================
// 13. HTTP Client for Paystack
// ==========================================
builder.Services.AddHttpClient<IPaystackService, PaystackService>(client =>
{
	client.BaseAddress = new Uri(builder.Configuration["Paystack:BaseUrl"] ?? "https://api.paystack.co");
	client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["Paystack:SecretKey"]}");
});

// ==========================================
// Build App
// ==========================================
var app = builder.Build();

// ==========================================
// Configure Middleware Pipeline
// ==========================================

// Swagger (Development only)
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// HTTPS Redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.MapHealthChecks("/health");

// Controllers
app.MapControllers();

// ==========================================
// Run Migrations on Startup
// ==========================================
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
	await dbContext.Database.MigrateAsync();
}

// ==========================================
// Run App
// ==========================================
app.Run();