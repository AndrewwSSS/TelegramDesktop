using CommonLibrary.Containers;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using System.Data.Entity;

namespace TelegramServer
{
    public class TelegramDb : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<GroupChat> GroupChats { get; set; }
        public DbSet<ImageContainer> Images { get; set; }
        public DbSet<FileContainer> Files { get; set; }
        public DbSet<PreparatoryUser> PreparatoryUsers { get; set; }

        public TelegramDb() : base("Telegram")
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupChat>().HasMany(g => g.Messages)
                                            .WithOptional()
                                            .WillCascadeOnDelete(true);

            modelBuilder.Entity<User>().HasMany(u => u.Chats).WithMany(g => g.Members);
        } 
    }
}
