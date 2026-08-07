using EstateHub.Contracts.Events;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserService.API.Middleware;
using UserService.Application.Commands.Auth;
using UserService.Application.Consumers;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Service;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Database Context
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UserDbContext>(options =>
	options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// ==========================================
// 2. Swagger Configuration
// ==========================================
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "UserService API",
		Version = "v1",
		Description = "EstateHub User Service - Authentication & User Management"
	});

	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Enter your JWT token below (no need to type 'Bearer ' — Swagger adds it automatically)."
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
// 3. Identity
// ==========================================
builder.Services
	.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
	{
		options.Password.RequireDigit = true;
		options.Password.RequireUppercase = true;
		options.Password.RequireLowercase = true;
		options.Password.RequiredLength = 8;
		options.User.RequireUniqueEmail = true;
	})
	.AddEntityFrameworkStores<UserDbContext>()
	.AddDefaultTokenProviders();

// ==========================================
// 4. CORS
// ==========================================
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll",
		builder =>
		{
			builder.AllowAnyOrigin()
				   .AllowAnyMethod()
				   .AllowAnyHeader();
		});
});

// ==========================================
// 5. MediatR
// ==========================================
builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
	cfg.RegisterServicesFromAssembly(typeof(RegisterAdminCommand).Assembly);
});
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ==========================================
// 6. JWT Authentication
// ==========================================
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtSecretKey))
{
	throw new InvalidOperationException(
		"Jwt:SecretKey is missing. Set it via appsettings, environment variable (Jwt__SecretKey), or Azure Container App secret.");
}

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidIssuer = jwtIssuer,
		ValidateAudience = true,
		ValidAudience = jwtAudience,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
		ValidateLifetime = true,
		ClockSkew = TimeSpan.FromMinutes(1)
	};
});

builder.Services.AddAuthorization();

// ==========================================
// 7. MassTransit (RabbitMQ) - ✅ NEW
// ==========================================
builder.Services.AddMassTransit(x =>
{
	// ✅ Register the consumer
	x.AddConsumer<PaymentProcessedConsumer>();

	x.UsingRabbitMq((ctx, cfg) =>
	{
		var host = builder.Configuration["MessageBroker:Host"];
		cfg.Host(new Uri(host), h => { });

		// ✅ Configure receive endpoint
		cfg.ReceiveEndpoint("payment-processed", e =>
		{
			e.ConfigureConsumer<PaymentProcessedConsumer>(ctx);
		});
	});
});



builder.Services.Configure<MassTransitHostOptions>(options =>
{
	options.WaitUntilStarted = true;
	options.StartTimeout = TimeSpan.FromSeconds(30);
	options.StopTimeout = TimeSpan.FromSeconds(30);
});

builder.Services.Configure<HostOptions>(options =>
{
	options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

// ==========================================
// 8. Controllers
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ==========================================
// Build App
// ==========================================
var app = builder.Build();

// ==========================================
// Middleware Pipeline
// ==========================================

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API v1");
	options.RoutePrefix = "swagger";
});

// Root redirect
app.MapGet("/", () => Results.Redirect("/swagger"));
// Add a test endpoint to publish a message
// ==========================================
// Test Endpoint for RabbitMQ
// ==========================================
app.MapPost("/test-publish", async (IPublishEndpoint publishEndpoint, ILogger<Program> logger) =>
{
	try
	{
		logger.LogInformation("🚀 Attempting to publish test message...");
		logger.LogInformation("🔑 Using connection: amqps://lcvhfmio:****@ostrich.lmq.cloudamqp.com/lcvhfmio");

		// Create and publish test event
		var testEvent = new TestEvent(
			"Hello from UserService at " + DateTime.UtcNow
		);

		logger.LogInformation("📤 Publishing event: {Message}", testEvent.Message);

		await publishEndpoint.Publish(testEvent);

		logger.LogInformation("✅ Publish method completed successfully");
		return Results.Ok(new { message = "✅ Test message published to RabbitMQ!" });
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "❌ Failed to publish test message");
		return Results.BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
	}

});
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

// ==========================================
// Role Seeding
// ==========================================
using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
	await SeedRolesAsync(roleManager);
}

async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
{
	string[] roles = { "Admin", "EstateManager", "Resident" };

	foreach (var role in roles)
	{
		if (!await roleManager.RoleExistsAsync(role))
		{
			await roleManager.CreateAsync(new IdentityRole<Guid>(role));
		}
	}
}

app.Run();