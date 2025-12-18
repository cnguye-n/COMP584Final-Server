using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace worldmodel;

public partial class Comp584Context : IdentityDbContext<WorldModelUser>
{
    public Comp584Context()
    {
    }

    public Comp584Context(DbContextOptions<Comp584Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Team> Teams { get; set; } = null!;
    public virtual DbSet<TeamMember> TeamMembers { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        IConfigurationBuilder ConfigureBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true);

        IConfigurationRoot configuration = ConfigureBuilder.Build();

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //keep this first so Identity tables are configured
        base.OnModelCreating(modelBuilder);

        // Team ↔ TeamMember relationship
        modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.Team)
            .WithMany(t => t.TeamMembers)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // User ↔ TeamMember relationship
        modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.User)
            .WithMany() // WorldModelUser does NOT need a TeamMembers collection
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Prevent duplicate memberships
        modelBuilder.Entity<TeamMember>()
            .HasIndex(tm => new { tm.TeamId, tm.UserId })
            .IsUnique();

        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
