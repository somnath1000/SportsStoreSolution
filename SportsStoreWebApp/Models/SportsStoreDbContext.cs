using Microsoft.EntityFrameworkCore;
using SportsStoreWebApp.Models.Entities;

namespace SportsStoreWebApp.Models
{
  public class SportsStoreDbContext : DbContext
  {
    public SportsStoreDbContext(DbContextOptions<SportsStoreDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
  }
}
