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

        modelBuilder.Entity<OrderItem>()
                    .Property(p=>p.UnitPrice)
                    .HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>()
                    .Property(p=>p.RowVersion)
                    .IsRowVersion();
        modelBuilder.Entity<OrderItem>()
                    .HasOne(o=>o.PaintProduct)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Order>()
                    .HasMany(o=>o.OrderItems)
                    .WithOne()
                    .HasForeignKey(o=>o.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PaintProduct>()
                    .Property(p=>p.Price)
                    .HasPrecision(18, 2);

        modelBuilder.Entity<PaintProduct>()
                    .HasIndex(p=>p.Name)
                    .IsUnique();
        
        modelBuilder.Entity<PaintProduct>()
                    .Property(p=>p.RowVersion)
                    .IsRowVersion();

        modelBuilder.Entity<User>()
                    .HasIndex(u=>u.Email)
                    .IsUnique();

        modelBuilder.Entity<Order>()
                    .HasOne(o=>o.User)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Order>()
                    .Property(o=>o.TotalPrice)
                    .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
                    .Property(o=>o.RowVersion)
                    .IsRowVersion();

        modelBuilder.Entity<User>()
                    .Property(u=>u.RowVersion)
                    .IsRowVersion();

    }
}
