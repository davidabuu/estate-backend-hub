using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using UserService.Application.Commands.Resident;
using UserService.Application.DTOS.Requests.Auth;
using UserService.Application.DTOS.Responses.Residents;
using UserService.Application.Enums;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handlers.Resident;

public class BulkUploadResidentCommandHandler(
	UserManager<ApplicationUser> userManager,
	UserDbContext dbContext) : IRequestHandler<BulkUploadResidentCommand, BulkResidentUploadResponse>
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly UserDbContext _dbContext = dbContext;

	public async Task<BulkResidentUploadResponse> Handle(BulkUploadResidentCommand command, CancellationToken ct)
	{
		var errors = new List<BulkResidentError>();
		var successCount = 0;
		var totalRecords = 0;

		// ✅ Check Estate
		var estate = await _dbContext.EstateRegistration
			.FirstOrDefaultAsync(e => e.Id == command.EstateId, ct);

		if (estate == null)
		{
			throw new Exception("Estate not found");
		}

		if (!estate.IsApproved)
		{
			throw new Exception("Estate is not approved. Please wait for admin approval before adding residents.");
		}

		// 1. Read Excel File
		using var stream = new MemoryStream();
		await command.ExcelFile.CopyToAsync(stream, ct);
		stream.Position = 0;

		using var package = new ExcelPackage(stream);
		var worksheet = package.Workbook.Worksheets[0];
		var rowCount = worksheet.Dimension?.Rows ?? 0;

		if (rowCount < 2)
		{
			throw new Exception("Excel file is empty or has no data rows.");
		}

		// 2. Loop through each row
		for (int row = 2; row <= rowCount; row++)
		{
			totalRecords++;

			try
			{
				// ✅ Read PropertyType as string and convert to enum
				var propertyTypeString = worksheet.Cells[row, 6]?.Text ?? string.Empty;
				var propertyType = ParsePropertyType(propertyTypeString);

				var residentDto = new ResidentRegistrationRequestDto
				{
					FirstName = worksheet.Cells[row, 1]?.Text ?? string.Empty,
					LastName = worksheet.Cells[row, 2]?.Text ?? string.Empty,
					Email = worksheet.Cells[row, 3]?.Text ?? string.Empty,
					PhoneNumber = worksheet.Cells[row, 4]?.Text ?? string.Empty,
					Password = worksheet.Cells[row, 5]?.Text ?? string.Empty,
					PropertyType = propertyType,  // ✅ Now using PropertyType enum
					MeterNumber = worksheet.Cells[row, 7]?.Text ?? string.Empty,
					HouseAddress = worksheet.Cells[row, 8]?.Text ?? string.Empty,
					EstateId = command.EstateId
				};

				// 3. Validate
				if (string.IsNullOrEmpty(residentDto.Email) || string.IsNullOrEmpty(residentDto.Password))
				{
					errors.Add(new BulkResidentError(row, residentDto.Email, "Email and Password are required"));
					continue;
				}

				// ✅ Validate PropertyType
				if (residentDto.PropertyType == 0)
				{
					errors.Add(new BulkResidentError(row, residentDto.Email, "Invalid Property Type. Must be: Detached, SemiDetached, Bungalow, Terrace, Private, Metro, Duplex"));
					continue;
				}

				// 4. Check if user exists
				var existingUser = await _userManager.FindByEmailAsync(residentDto.Email);
				if (existingUser != null)
				{
					errors.Add(new BulkResidentError(row, residentDto.Email, "User already exists"));
					continue;
				}

				// 5. Create ApplicationUser
				var user = new ApplicationUser
				{
					Id = Guid.NewGuid(),
					UserName = residentDto.Email,
					Email = residentDto.Email,
					FullName = $"{residentDto.FirstName} {residentDto.LastName}",
					CreatedAt = DateTime.UtcNow,
					IsActive = true
				};

				var result = await _userManager.CreateAsync(user, residentDto.Password);

				if (!result.Succeeded)
				{
					var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
					errors.Add(new BulkResidentError(row, residentDto.Email, errorMsg));
					continue;
				}

				await _userManager.AddToRoleAsync(user, "Resident");

				// 6. Create Resident with PropertyType
				var resident = new ResidentRegistration
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					EstateId = command.EstateId,
					FirstName = residentDto.FirstName,
					LastName = residentDto.LastName,
					Email = residentDto.Email,
					PhoneNumber = residentDto.PhoneNumber,
					PropertyType = residentDto.PropertyType,  // ✅ Using PropertyType enum
					MeterNumber = residentDto.MeterNumber,
					HouseAddress = residentDto.HouseAddress,
					IsActive = true,
					IsRegistered = true,
					CreatedAt = DateTime.UtcNow
				};

				await _dbContext.ResidentRegistration.AddAsync(resident, ct);
				await _dbContext.SaveChangesAsync(ct);

				successCount++;
			}
			catch (Exception ex)
			{
				errors.Add(new BulkResidentError(row, "Unknown", ex.Message));
			}
		}

		return new BulkResidentUploadResponse(
			TotalRecords: totalRecords,
			SuccessCount: successCount,
			FailedCount: errors.Count,
			Errors: errors
		);
	}

	// ✅ Helper method to parse PropertyType from string
	private PropertyType ParsePropertyType(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return 0;

		// Try parsing by enum name (case insensitive)
		if (Enum.TryParse<PropertyType>(value, true, out var result))
			return result;

		// Try parsing by number
		if (int.TryParse(value, out var intValue) && Enum.IsDefined(typeof(PropertyType), intValue))
			return (PropertyType)intValue;

		return 0; // Invalid
	}
}