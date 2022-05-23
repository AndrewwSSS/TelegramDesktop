using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;

namespace TelegramServer
{
    public class TelegramDb : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<GroupChat> GroupChats { get; set; }
        //public DbContext Me

        public TelegramDb() : base("Telegram")
        {
   
            //Database.SetInitializer<TelegramDb>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
