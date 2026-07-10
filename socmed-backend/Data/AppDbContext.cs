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
    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

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

        modelBuilder.Entity<RantReply>()
            .HasOne(r => r.ParentReply)
            .WithMany(r => r.ChildReplies)
            .HasForeignKey(r => r.ParentReplyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RantReply>()
            .HasIndex(r => r.ParentReplyId);

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

        // ConversationParticipant Composite Key
        modelBuilder.Entity<ConversationParticipant>()
            .HasKey(cp => new { cp.ConversationId, cp.UserId });

        modelBuilder.Entity<ConversationParticipant>()
            .HasOne(cp => cp.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConversationParticipant>()
            .HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Message relationships & index
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasIndex(m => new { m.ConversationId, m.CreatedAt });
    }
}

