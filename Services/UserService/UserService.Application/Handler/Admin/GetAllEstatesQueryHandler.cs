using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs.Responses.Admin;
using UserService.Application.Queries.Admin;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Admin;

public class GetAllEstatesQueryHandler(UserDbContext dbContext) : IRequestHandler<GetAllEstatesQuery, List<AdminEstateListResponse>>
{
	private readonly UserDbContext _dbContext = dbContext;

	public async Task<List<AdminEstateListResponse>> Handle(GetAllEstatesQuery query, CancellationToken ct)
	{
		var estates = await _dbContext.EstateRegistration
			.Include(e => e.User)
			.OrderByDescending(e => e.CreatedAt)
			.ToListAsync(ct);

		var responses = new List<AdminEstateListResponse>();

		foreach (var estate in estates)
		{
			// Get resident count
			var residentCount = await _dbContext.ResidentRegistration
				.CountAsync(r => r.EstateId == estate.Id && r.IsActive, ct);

			// Get dues count
			var duesCount = await _dbContext.EstateDue
				.CountAsync(d => d.EstateId == estate.Id, ct);

			responses.Add(new AdminEstateListResponse(
				EstateId: estate.Id,
				EstateName: estate.EstateName,
				EstateAddress: estate.EstateAddress,
				EstateState: estate.EstateState,
				
				AdminEmail: estate.User?.Email ?? "N/A",
				AdminFullName: estate.User?.FullName ?? "N/A",
				IsApproved: estate.IsApproved,
				CreatedAt: estate.CreatedAt,
				ApprovedAt: estate.ApprovedAt,
				TotalResidents: residentCount,
				TotalDues: duesCount
			));
		}

		return responses;
	}
}