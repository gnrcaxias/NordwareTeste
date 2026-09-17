using Nordware.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nordware.Infrastructure.Configurations;

namespace Nordware.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public DbSet<Reservation> Reservations  { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());


    }
}