using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs.Responses.Dues;
using UserService.Application.Queries.Dues;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Dues;

public class GetEstateDuesQueryHandler : IRequestHandler<GetEstateDuesQuery, List<EstateDueResponse>>
{
	private readonly UserDbContext _dbContext;

	public GetEstateDuesQueryHandler(UserDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<List<EstateDueResponse>> Handle(GetEstateDuesQuery query, CancellationToken ct)
	{
		
		var estateDues = await _dbContext.EstateDue
			.Where(e => e.EstateId == query.EstateId)
			.OrderByDescending(e => e.CreatedAt)
			.ToListAsync(ct);

		if (estateDues.Count == 0)
		{
			return new List<EstateDueResponse>();
		}

		var responses = new List<EstateDueResponse>();

		foreach (var due in estateDues)
		{
			// 2. Get all ResidentDues for this EstateDue
			var residentDues = await _dbContext.ResidentDues
				.Where(r => r.EstateDueId == due.Id)
				.ToListAsync(ct);

			// 3. Calculate statistics
			var totalResidents = residentDues.Count;
			var paidCount = residentDues.Count(r => r.IsPaid);
			var pendingCount = residentDues.Count(r => r.Status == DueStatus.Pending);
			var overdueCount = residentDues.Count(r => r.Status == DueStatus.Overdue);

			// 4. Get amount from first resident (since all residents have the same amount for a due)
			var amount = residentDues.FirstOrDefault()?.Amount ?? 0;

			responses.Add(new EstateDueResponse(
				Id: due.Id,
				DueName: due.DueName!,
				Description: due.Description,
				Amount: amount,
				DueType: due.DueType,
				DueDate: due.DueDate,
				PropertyTypeAmounts: due.PropertyTypeAmounts,
				TotalResidents: totalResidents,
				PaidCount: paidCount,
				PendingCount: pendingCount,
				OverdueCount: overdueCount,
				IsActive: due.IsActive,
				CreatedAt: due.CreatedAt,
				UpdatedAt: due.UpdatedAt
			));
		}

		return responses;
	}
}