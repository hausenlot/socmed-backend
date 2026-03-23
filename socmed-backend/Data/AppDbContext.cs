using Microsoft.EntityFrameworkCore;
using socmed_backend.Models;

namespace socmed_backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Rant> Rants { get; set; } = null!;
    public DbSet<RantLike> RantLikes { get; set; } = null!;
    public DbSet<RantReRant> RantReRants { get; set; } = null!;
    public DbSet<RantBookmark> RantBookmarks { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    
    public DbSet<RantReply> RantReplies { get; set; } = null!;
    public DbSet<Follow> Follows { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<ReplyLike> ReplyLikes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Apply global query filter for soft delete
        modelBuilder.Entity<Rant>().HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<Rant>()
            .HasIndex(r => r.PublicId)
            .IsUnique();

        modelBuilder.Entity<RantReply>()
            .HasIndex(rr => rr.PublicId)
            .IsUnique();

        // Configure Composite Primary Keys
        modelBuilder.Entity<RantLike>()
            .HasKey(l => new { l.RantId, l.UserId });

        modelBuilder.Entity<RantReRant>()
            .HasKey(r => new { r.RantId, r.UserId });

        modelBuilder.Entity<RantBookmark>()
            .HasKey(b => new { b.RantId, b.UserId });

        modelBuilder.Entity<ReplyLike>()
            .HasKey(rl => new { rl.ReplyId, rl.UserId });

        // Configure Comp. Key and relations for Follow
        modelBuilder.Entity<Follow>()
            .HasKey(f => new { f.FollowerId, f.FollowingId });

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Following) // A User follows many Users
            .HasForeignKey(f => f.FollowerId);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Following)
            .WithMany(u => u.Followers) // A User is followed by many Users
            .HasForeignKey(f => f.FollowingId);

        // Apply global query filter for soft delete of RantReply
        modelBuilder.Entity<RantReply>().HasQueryFilter(p => !p.IsDeleted);

        // Notification indexing
        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.UserId);
    }
}

