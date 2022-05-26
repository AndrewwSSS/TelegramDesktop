using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using CommonLibrary.Containers;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;

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
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<GroupChat>().HasMany(g => g.Messages)
            .WithOptional()
            .WillCascadeOnDelete(true);
        }
    }
}
