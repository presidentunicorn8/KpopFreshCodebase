using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpopFresh.Model;
using HtmlAgilityPack;
using System.Web;
using System.Net.Http.Json;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;

namespace KpopFresh.Services
{
    public class WebScraper
    {
        

        public async Task<List<Song>> GetItems(DateOnly todayDate)
        {

            string monthNumber = todayDate.Month.ToString(); 
            var GitHubRawFileUrl = @$"https://raw.githubusercontent.com/presidentunicorn8/KpopFreshScraping/main/data-{monthNumber}.json";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(GitHubRawFileUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    List<Song> songList = JsonConvert.DeserializeObject<List<Song>>(jsonData);
                    return songList;
                }
                else
                {
                    // Handle the case when fetching data from GitHub fails
                    throw new Exception("Failed to fetch data from GitHub.");
                }
            }
        }
    }
}
