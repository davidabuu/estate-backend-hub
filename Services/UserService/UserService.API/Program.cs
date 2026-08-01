using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Auth;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Service;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UserDbContext>(options =>
	options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// 2. Identity
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

// 3. CORS (Define the policy, don't use it yet)
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

// 4. MediatR
builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);           // API assembly
	cfg.RegisterServicesFromAssembly(typeof(RegisterAdminCommand).Assembly); // Application assembly
});
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// 5. Controllers
builder.Services.AddControllers();

// 6. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---------------------------------------------------------
// MIDDLEWARE PIPELINE (Order Matters!)
// ---------------------------------------------------------

// 1. Swagger (Development only)
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// 2. HTTPS
app.UseHttpsRedirection();

// 3. CORS - ❗ ONLY CALLED ONCE, before Auth
app.UseCors("AllowAll");

// 4. Auth
app.UseAuthentication();
app.UseAuthorization();

// 5. Controllers
app.MapControllers();

// ---------------------------------------------------------
// Role Seeding
// ---------------------------------------------------------
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