using Microsoft.EntityFrameworkCore;
using System;
using Wavlo.Models;

namespace Wavlo.Data
{
    public class ChatDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatUser>().HasKey(x => new { x.ChatId, x.UserId });
        }
    }
}
