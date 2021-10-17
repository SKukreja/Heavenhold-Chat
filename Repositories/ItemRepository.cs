using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using HeavenholdBot.Models;


namespace HeavenholdBot.Repositories
{
    public class ItemRepository
    {
        private List<ItemInfo> iteminfoList;

        public ItemRepository()
        {
            // Cache the API response in a local JSON file
            if (!File.Exists(@"Items.json"))
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        var json = client.DownloadString(System.Configuration.ConfigurationManager.AppSettings["itemAPIEndpoint"]);
                        File.WriteAllText(@"Items.json", json);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            iteminfoList = JsonConvert.DeserializeObject<List<ItemInfo>>(File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["itemCacheFileLocation"]));
        }

        public void RefreshData()
        {
            // Cache the API response in a local JSON file
            if (File.Exists(@"Items.json"))
            {
                try
                {
                    File.Delete(@"Items.json");
                    using (var client = new WebClient())
                    {
                        var json = client.DownloadString(System.Configuration.ConfigurationManager.AppSettings["itemAPIEndpoint"]);
                        File.WriteAllText(@"Items.json", json);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            iteminfoList = JsonConvert.DeserializeObject<List<ItemInfo>>(File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["itemCacheFileLocation"]));
        }

        public ItemInfo GetItem(string[] namelist)
        {
            KeyValuePair<ItemInfo, int> selectedItem = new KeyValuePair<ItemInfo, int>();
            Dictionary<ItemInfo, int> itemPreference = new Dictionary<ItemInfo, int>();
            string[] titleList;
            int weightedCount = 0;

            try
            {
                // For every item within the JSON response
                foreach (ItemInfo itemInfo in iteminfoList)
                {
                    weightedCount = 0;
                    // loop through each word in the search query
                    foreach (string namecheck in namelist)
                    {
                        titleList = itemInfo.Title.Rendered.Replace("&#8217;", "'").Split(' ');

                        // and see how many words in the item's name match
                        if (itemInfo.Title.Rendered.Replace("&#8217;", "'").ToLower().Equals(namecheck.Replace("&#8217;", "'").ToLower()))
                        {
                            weightedCount += 1;
                        }
                        foreach (string slugcheck in titleList)
                        {
                            if (slugcheck.Replace("&#8217;", "'").ToLower().Equals(namecheck.Replace("&#8217;", "'").ToLower()))
                            {
                                weightedCount += 1;
                            }
                        }
                    }
                    if (weightedCount > 0)
                        itemPreference.Add(itemInfo, weightedCount);
                }
                foreach (KeyValuePair<ItemInfo, int> kvp in itemPreference)
                {
                    if (selectedItem.Value < kvp.Value)
                        selectedItem = kvp;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return selectedItem.Key;
        }
    }
}