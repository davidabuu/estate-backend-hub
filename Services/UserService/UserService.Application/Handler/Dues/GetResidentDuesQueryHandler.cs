using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs.Responses.Dues;
using UserService.Application.Queries.Dues;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Dues;

public class GetResidentDuesQueryHandler : IRequestHandler<GetResidentDuesQuery, List<ResidentDueResponse>>
{
	private readonly UserDbContext _dbContext;

	public GetResidentDuesQueryHandler(UserDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<List<ResidentDueResponse>> Handle(GetResidentDuesQuery query, CancellationToken ct)
	{
		// 1. Get all ResidentDues for the resident
		var residentDues = await _dbContext.ResidentDues
			.Where(r => r.ResidentId == query.ResidentId)
			.OrderByDescending(r => r.DueDate)
			.ToListAsync(ct);

		if (residentDues.Count == 0)
		{
			return new List<ResidentDueResponse>();
		}

		var responses = new List<ResidentDueResponse>();

		foreach (var due in residentDues)
		{
			// 2. Calculate days until due
			var daysUntilDue = (due.DueDate - DateTime.UtcNow.Date).Days;

			// 3. Check if overdue
			var isOverdue = due.Status == DueStatus.Overdue ||
				(due.DueDate < DateTime.UtcNow.Date && !due.IsPaid);

			// 4. Update status if overdue
			if (isOverdue && due.Status != DueStatus.Overdue)
			{
				due.Status = DueStatus.Overdue;
				await _dbContext.SaveChangesAsync(ct);
			}

			responses.Add(new ResidentDueResponse(
				Id: due.Id,
				EstateDueId: due.EstateDueId,
				DueName: due.DueName!,
				Description: due.Description,
				Amount: due.Amount,
				DueType: due.DueType,
				DueDate: due.DueDate,
				Status: due.Status,
				IsPaid: due.IsPaid,
				PropertyType: due.PropertyType,
				PaidAt: due.PaidAt,
				PaymentReference: due.PaymentReference,
				DaysUntilDue: daysUntilDue,
				IsOverdue: isOverdue,
				CreatedAt: due.CreatedAt
			));
		}

		return responses;
	}
}