using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TerminalCommandEntity> TerminalCommands => Set<TerminalCommandEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TerminalCommandEntity>(b =>
        {
            b.ToTable("terminal_commands");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Key).IsUnique();
            b.Property(x => x.Key).IsRequired();
            b.Property(x => x.LabelPt).IsRequired();
            b.Property(x => x.LabelEn).IsRequired();
            b.Property(x => x.Order).HasColumnName("order_int");
            b.Property(x => x.Enabled);
            b.Property(x => x.Steps).HasColumnType("jsonb");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
    }
}

public sealed class TerminalCommandEntity
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string LabelPt { get; set; } = string.Empty;
    public string LabelEn { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool Enabled { get; set; } = true;
    public string Steps { get; set; } = "[]"; // JSON (compatível com jsonb)
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}


