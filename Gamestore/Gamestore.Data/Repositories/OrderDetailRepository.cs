using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

public class OrderDetailRepository(GameCatalogDbContext context)
    : Repository<OrderDetail>(context), IOrderDetailRepository
{
    private readonly GameCatalogDbContext _context = context;

    public override async Task<IEnumerable<OrderDetail>> GetAllAsync()
    {
        return await _context.OrderDetails
            .AsNoTracking()
            .Include(od => od.Order)
            .Include(od => od.Game)
            .ToListAsync();
    }
}