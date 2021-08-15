using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace CivSim
{
    public class CivManager
    {
        public static CivManager Instance;
        public CivManager()
        {
            DateTime now = DateTime.Today.AddHours(7);
            int daysToUpdate = (7 - (int)now.DayOfWeek) % 7;
            NextUpdate = now.AddDays(daysToUpdate > 0 ? daysToUpdate : 7);
        }

        [JsonInclude]
        public Dictionary<string, Civ> Civs = new Dictionary<string, Civ>();

        public DateTime NextUpdate { get; set; }

        public static async Task Save()
        {
            string saveData = JsonSerializer.Serialize(Instance);

            await File.WriteAllTextAsync("data/save.json", saveData);
        }

        public static async Task Load()
        {
            if (File.Exists("data/save.json"))
            {
                string saveData = await File.ReadAllTextAsync("data/save.json");
                Instance = JsonSerializer.Deserialize<CivManager>(saveData);
            }
            else
            {
                Instance = new CivManager();
            }
        }

        public void UpdateCivs()
        {
            foreach (Civ c in Civs.Values)
            {
                c.Update();
            }
        }
    }
}