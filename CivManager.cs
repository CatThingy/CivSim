using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;


namespace CivSim
{
    public enum Stat
    {
        Offence,
        Defence,
        Research,
        Education,
        Healthcare,
        Civilian,
        Morale
    }

    public class CivManager
    {
        public static CivManager Instance
        {
            get
            {
                if (_Instance != null)
                {
                    return _Instance;
                }
                _Instance = new CivManager();
                return _Instance;
            }
        }

        private static CivManager _Instance;
        public CivManager()
        {
            DateTime now = DateTime.Today.AddHours(7);
            int daysToUpdate = (7 - (int)now.DayOfWeek) % 7;
            NextUpdate = now.AddDays(daysToUpdate > 0 ? daysToUpdate : 7);
        }

        [JsonInclude]
        public Dictionary<string, Civ> Civs = new Dictionary<string, Civ>();

        public DateTime NextUpdate { get; set; }

        public async Task Save()
        {
            string saveData = JsonSerializer.Serialize(Instance);

            await File.WriteAllTextAsync("data/save.json", saveData);
        }

        public async Task Load()
        {
            if (File.Exists("data/save.json"))
            {
                string saveData = await File.ReadAllTextAsync("data/save.json");
                _Instance = JsonSerializer.Deserialize<CivManager>(saveData);
            }
            else
            {
                _Instance = new CivManager();
            }
        }
        public void CheckForUpdates()
        {
            while (NextUpdate <= DateTime.Now)
            {
                foreach (Civ c in Civs.Values)
                {
                    c.Update();
                }

                NextUpdate = NextUpdate.AddDays(7);
            }
        }
        public static string GetUserHash(ulong id)
        {
            HashAlgorithm algorithm = SHA1.Create();
            byte[] rawHash = algorithm.ComputeHash(BitConverter.GetBytes(id));
            var sb = new StringBuilder();
            foreach (byte b in rawHash)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString()[^8..];
        }

        public bool UserExists(string userHash)
        {
            return CivManager.Instance.Civs.ContainsKey(userHash);
        }
    }
}