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
                    .HasMany(o=>o.PaintProducts)
                    .WithMany()
                    .UsingEntity(
                        l=>l.HasOne(typeof(PaintProduct))
                            .WithMany()
                            .OnDelete(DeleteBehavior.Restrict),
                        r=>r.HasOne(typeof(Order))
                            .WithMany()
                            .OnDelete(DeleteBehavior.Restrict) 
                    );

        modelBuilder.Entity<User>()
                    .Property(u=>u.RowVersion)
                    .IsRowVersion();

    }
}
