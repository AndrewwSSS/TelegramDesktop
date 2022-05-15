using System.Data.Entity;
using CommonLibrary;

namespace TelegramServer
{
    public class TelegramDb : DbContext
    {


        public DbSet<User> Users { get; set; }
        public DbSet<GroupChat> GroupChats { get; set; }


        public TelegramDb()
            : base("Telegram")
        {
            
            //Database.SetInitializer<TelegramDb>(null);
        }
    }
}
