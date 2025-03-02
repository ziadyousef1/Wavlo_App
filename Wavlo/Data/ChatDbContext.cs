using Microsoft.EntityFrameworkCore;
using System;
using Wavlo.Models;

namespace Wavlo.Data
{
    public class ChatDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
            
        }
    }
}
