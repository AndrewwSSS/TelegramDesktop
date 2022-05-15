using System.Data.Entity;
using CommonLibrary;

namespace Server
{
    public class DataBaseContext : DbContext
    {


        public DbSet<User> Users { get; set; }
        public DbSet<GroupChat> GroupChats { get; set; }


        public DataBaseContext()
            : base(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Telegram;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
        {
                       
        }
    }
}
