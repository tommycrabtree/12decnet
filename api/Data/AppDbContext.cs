using api.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<DateBatch> DateBatches { get; set; }
    public DbSet<ReplenishmentSession> ReplenishmentSessions { get; set; }
}
