using Microsoft.EntityFrameworkCore;
using TradeBlotter.Api.Models;

namespace TradeBlotter.Api.Data;

public class TradeDbContext : DbContext
{
    public TradeDbContext(DbContextOptions<TradeDbContext> options) : base(options) { }

    public DbSet<Trade> Trades => Set<Trade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trade>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Symbol).IsRequired().HasMaxLength(20);
            entity.Property(t => t.Side).HasConversion<string>();
            entity.Property(t => t.Quantity).HasPrecision(18, 4);
            entity.Property(t => t.Price).HasPrecision(18, 4);
            entity.Property(t => t.Timestamp).IsRequired();
        });
    }
}
