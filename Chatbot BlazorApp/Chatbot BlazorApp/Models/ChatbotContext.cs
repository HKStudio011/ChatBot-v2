using Microsoft.EntityFrameworkCore;

namespace Chatbot_BlazorApp.Models
{
    public class ChatbotContext : DbContext
    {
        public DbSet<Contents> Contents { get; set; }
        public DbSet<SplitContents> SplitContents { get; set; }
        public DbSet<Keywords> Keywords { get; set; }

        public ChatbotContext(DbContextOptions<ChatbotContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var item in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = item.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    item.SetTableName(tableName.Substring(6));// or Replace("AspNet","")
                }
            }
        }
    }
}
