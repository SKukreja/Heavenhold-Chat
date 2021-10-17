using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using HeavenholdBot.Models;
using System.Text.RegularExpressions;

namespace HeavenholdBot.Repositories
{
    public class HeroRepository
    {
        private List<HeroInfo> heroinfoList;

        public HeroRepository()
        {
            // Cache the JSON response in a local file
            if (!File.Exists(@"Heroes.json"))
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        var json = client.DownloadString(System.Configuration.ConfigurationManager.AppSettings["heroAPIEndpoint"]);
                        json = Regex.Replace(json, "\\\\r\\\\n", "");
                        File.WriteAllText(@"Heroes.json", json);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            // Deserialize the JSON objects into a list of a class instances
            string jsonResponse = File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["heroCacheFileLocation"]);
            heroinfoList = JsonConvert.DeserializeObject<List<HeroInfo>>(jsonResponse);
            heroinfoList.RemoveAll(x => x.Acf.BioFields == null);
        }

        public void RefreshData()
        {
            // Cache the JSON response in a local file
            if (File.Exists(@"Heroes.json"))
            {
                try
                {
                    File.Delete(@"Heroes.json");
                    using (var client = new WebClient())
                    {
                        var json = client.DownloadString(System.Configuration.ConfigurationManager.AppSettings["heroAPIEndpoint"]);
                        json = Regex.Replace(json, "\\\\r\\\\n", "");
                        File.WriteAllText(@"Heroes.json", json);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            // Deserialize the JSON objects into a list of a class instances
            string jsonResponse = File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["heroCacheFileLocation"]);
            heroinfoList = JsonConvert.DeserializeObject<List<HeroInfo>>(jsonResponse);
            heroinfoList.RemoveAll(x => x.Acf.BioFields == null);
        }

        public HeroInfo GetHero(string[] namelist)
        {
            KeyValuePair<HeroInfo, int> selectedHero = new KeyValuePair<HeroInfo, int>();
            Dictionary<HeroInfo, int> heroPreference = new Dictionary<HeroInfo, int>();
            string[] titleList;
            int weightedCount = 0;

            try
            {
                // For each hero in the JSON response
                foreach (HeroInfo heroInfo in heroinfoList)
                {
                    weightedCount = 0;
                    // loop through each word in the search query
                    foreach (string namecheck in namelist)
                    {
                        titleList = heroInfo.Title.Rendered.Split(' ');

                        // and see how many words match the hero's name and title
                        if (heroInfo.Acf.BioFields.Name.ToLower().Equals(namecheck.ToLower()))
                        {
                            weightedCount += 1;
                        }
                        foreach (string slugcheck in titleList)
                        {
                            if (slugcheck.ToLower().Equals(namecheck.ToLower()))
                            {
                                weightedCount += 1;
                            }
                        }
                    }
                    if (weightedCount > 0)
                        heroPreference.Add(heroInfo, weightedCount);
                }
                foreach (KeyValuePair<HeroInfo, int> kvp in heroPreference)
                {
                    if (selectedHero.Value < kvp.Value)
                        selectedHero = kvp;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return selectedHero.Key;
        }
    }
}