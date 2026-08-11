using System;
using Microsoft.EntityFrameworkCore;
using PaintStore.Models;

namespace PaintStore.API.Database;

public class PaintStoreDbContext: DbContext
{
    public DbSet<User> Users {get; set;}
    public DbSet<PaintProduct> PaintProducts {get; set;}
    public DbSet<Order> Orders {get; set;}

    public PaintStoreDbContext(DbContextOptions<PaintStoreDbContext> dbContextOptions): base(dbContextOptions)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PaintProduct>()
                    .Property(p=>p.Price)
                    .HasPrecision(18, 2);
    }
}
