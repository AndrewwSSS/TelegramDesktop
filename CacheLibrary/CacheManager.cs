using CommonLibrary.Containers;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibrary
{
    public class CacheManager
    {

        private static CacheManager instance;
        public static CacheManager Instance => instance ?? (instance = new CacheManager()); 
            
        private CacheManager()
        {
            Directory.CreateDirectory(Path.Combine(CachePath, DIR_USERS));
            Directory.CreateDirectory(Path.Combine(CachePath, DIR_GROUPS));
            Directory.CreateDirectory(Path.Combine(CachePath, DIR_LOGIN));
        }

        public string CachePath { get; set; } = "Cache\\";

        private static BinaryFormatter bf = new BinaryFormatter();

        private const string DIR_USERS = "Users\\";
        private const string DIR_GROUPS = "Groups\\";
        #region Users
        public void SaveUser(UserItemWrap user)
        {
            UserContainer container = new UserContainer(user);
            Save(Path.Combine(DIR_USERS, $"{user.User.Id}.bin"), user);
        }
        public UserItemWrap LoadUser(int id)
        {
            var fileName = Path.Combine(DIR_USERS, $"{id}.bin");
            return Load<UserContainer>(fileName).ToWrap();
        }
        private UserItemWrap LoadUser(string path) => Load<UserItemWrap>(Path.Combine(DIR_USERS, path));
        public List<UserItemWrap> LoadAllUsers()
        {
            var result = new List<UserItemWrap>();
            foreach (var fileName in Directory.GetFiles(Path.Combine(CachePath, DIR_USERS), "*.bin")) {
                result.Add(LoadUser(Path.GetFileName(fileName)));
            };
            return result;
        }
        #endregion

        #region Groups
        public void SaveGroup(PublicGroupInfo group) => Save(Path.Combine(DIR_GROUPS, $"{group.Id}.bin"), group);
        public UserItemWrap LoadGroup(int id)
        {
            var fileName = Path.Combine(DIR_GROUPS, $"{id}.bin");
            return Load<UserItemWrap>(fileName);
        }
        private PublicGroupInfo LoadGroup(string path) => Load<PublicGroupInfo>(Path.Combine(DIR_GROUPS, path));
        public List<PublicGroupInfo> LoadAllGroups()
        {
            var result = new List<PublicGroupInfo>();
            foreach (var fileName in Directory.GetFiles(Path.Combine(CachePath, DIR_GROUPS), "*.bin"))
            {
                result.Add(LoadGroup(Path.GetFileName(fileName)));
            };
            return result;
        }
        #endregion

        
        private void Save(string fileName, object obj)
        {
            if (string.IsNullOrEmpty(CachePath))
                throw new NullReferenceException("CachePath не задан или пуст");
            if (obj == null)
                throw new ArgumentNullException("Попытка кэшировать null");

            string path = Path.Combine(CachePath, fileName);
            using (FileStream writer = new FileStream(path, FileMode.OpenOrCreate))
                bf.Serialize(writer, obj);
        }
        private T Load<T>(string fileName)
        {
            if (string.IsNullOrEmpty(CachePath))
                throw new NullReferenceException("CachePath не задан или пуст");
            
            string path = Path.Combine(CachePath, fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Файла {path} нет в директории {CachePath}.");
            
            using (FileStream reader = new FileStream(path, FileMode.Open))
                return (T)bf.Deserialize(reader);
        }

        const string DIR_LOGIN = "Login\\";
        public void SaveGuid(string guid)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(CachePath, DIR_LOGIN, "fast_login_guid.txt")))
                writer.Write(guid);
        }
        public string LoadGuid()
        {
            if (!File.Exists(Path.Combine(CachePath, DIR_LOGIN, "fast_login_guid.txt")))
                return null;
            using (StreamReader reader = new StreamReader(Path.Combine(CachePath, DIR_LOGIN, "fast_login_guid.txt")))
                return reader.ReadToEnd();
        }
        public void SaveUserId(int id)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(CachePath, DIR_LOGIN, "user_id.txt")))
                writer.Write(id);
        }
        public int LoadUserId()
        {
            int result = -1;
            if (!File.Exists(Path.Combine(CachePath, DIR_LOGIN, "user_id.txt")))
                return result;
            using (StreamReader reader = new StreamReader(Path.Combine(CachePath, DIR_LOGIN, "user_id.txt")))
                int.TryParse(reader.ReadToEnd(), out result);
            return result;
        }

    }
}
