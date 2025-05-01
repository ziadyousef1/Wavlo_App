using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using Wavlo.Models;

namespace Wavlo.Data
{
    public class ChatDbContext : IdentityDbContext<User>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<StoryView> StoryViews { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatUser>().HasKey(x => new { x.ChatId, x.UserId });

            modelBuilder.Entity<UserImage>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserImages)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.Chats)
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StoryView>()
                .HasOne(sv => sv.Story)
                .WithMany()
                .HasForeignKey(sv => sv.StoryId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
