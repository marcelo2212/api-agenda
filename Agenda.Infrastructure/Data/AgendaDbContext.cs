using Agenda.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Infrastructure.Data;

public class AgendaDbContext : DbContext
{
    public AgendaDbContext(DbContextOptions<AgendaDbContext> options)
        : base(options) { }

    public DbSet<Contact> Contacts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("contacts");

            entity.HasKey(e => e.Id);

            entity
                .Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);

            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(100);

            entity.Property(e => e.Phone).HasColumnName("phone").IsRequired().HasMaxLength(20);

            entity
                .Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
