using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace CivSim
{
    public static class CivManager
    {
        public static Dictionary<string, Civ> Civs = new Dictionary<string, Civ>();

        public static async Task SaveCivs()
        {
            string CivData = JsonSerializer.Serialize(Civs);

            await File.WriteAllTextAsync("data/civs.json", CivData);
        }

        public static async Task LoadCivs()
        {
            string CivData = await File.ReadAllTextAsync("data/civs.json");

            try
            {
                Civs = JsonSerializer.Deserialize<Dictionary<string, Civ>>(CivData);
            }
            catch
            {
                Console.WriteLine("Could not load save data");
            }
        }

        public static void UpdateCivs()
        {
            foreach (Civ c in Civs.Values)
            {
                c.Update();
            }
        }
    }
}