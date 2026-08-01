using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs.Responses.Admin;
using UserService.Application.Enums;
using UserService.Application.Queries.Admin;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Admin;

public class GetEstateByIdQueryHandler : IRequestHandler<GetEstateByIdQuery, AdminEstateDetailResponse>
{
	private readonly UserDbContext _dbContext;

	public GetEstateByIdQueryHandler(UserDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<AdminEstateDetailResponse> Handle(GetEstateByIdQuery query, CancellationToken ct)
	{
		var estate = await _dbContext.EstateRegistration
			.Include(e => e.User)
			.FirstOrDefaultAsync(e => e.Id == query.EstateId, ct);

		if (estate == null)
		{
			throw new Exception("Estate not found");
		}

		// Get residents
		var residents = await _dbContext.ResidentRegistration
			.Where(r => r.EstateId == estate.Id && r.IsActive)
			.ToListAsync(ct);

		var residentSummaries = residents.Select(r => new ResidentSummary(
			Id: r.Id,
			FullName: $"{r.FirstName} {r.LastName}",
			Email: r.Email ?? "N/A",
			PhoneNumber: r.PhoneNumber ?? "N/A",
			PropertyType: r.PropertyType.ToString(),
			JoinedAt: r.CreatedAt
		)).ToList();

		// Get dues
		var dues = await _dbContext.EstateDue
			.Where(d => d.EstateId == estate.Id)
			.ToListAsync(ct);

		var dueSummaries = new List<DueSummary>();

		foreach (var due in dues)
		{
			var residentDues = await _dbContext.ResidentDues
				.Where(rd => rd.EstateDueId == due.Id)
				.ToListAsync(ct);

			dueSummaries.Add(new DueSummary(
				Id: due.Id,
				DueName: due.DueName!,
				Amount: due.PropertyTypeAmounts.FirstOrDefault().Value,
				DueType: due.DueType.ToString(),
				DueDate: due.DueDate,
				TotalResidents: residentDues.Count,
				PaidCount: residentDues.Count(rd => rd.IsPaid),
				PendingCount: residentDues.Count(rd => rd.Status == DueStatus.Pending),
				OverdueCount: residentDues.Count(rd => rd.Status == DueStatus.Overdue)
			));
		}

		return new AdminEstateDetailResponse(
			EstateId: estate.Id,
			EstateName: estate.EstateName,
			EstateAddress: estate.EstateAddress,
			
			PropertyTypes: estate.PropertyTypes ?? new List<PropertyType>(),
			
			BankName: estate.BankName!,
			AccountName: estate.AccountName!,
			AccountNumber: estate.AccountNumber!,
			BankCode: estate.BankCode!,
		
			EstateRegistrationDocUrl: estate.EstateRegistrationDocUrl,
			EstateAssociationRegistrationDocUrl: estate.EstateAssociationRegistrationDocUrl,
			IsApproved: estate.IsApproved,
			
			CreatedAt: estate.CreatedAt,
			ApprovedAt: estate.ApprovedAt,
			TotalResidents: residents.Count,
			TotalDues: dues.Count,
			Residents: residentSummaries,
			Dues: dueSummaries
		);
	}
}