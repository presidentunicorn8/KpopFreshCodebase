using KpopFresh.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpopFresh.Services;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;

namespace KpopFresh.ViewModel
{
    public partial class SongsViewModel : BaseViewModel
    {
        WebScraper webScraper;

        public ObservableCollection<Song> Songs { get; set; } = new();
        public ObservableCollection<Song> SongsNoFilter { get; set; } = new();


        public Command GetSongsCommand { get; }

        public SongsViewModel(WebScraper webScraper)
        {
            GetSongsCommand = new Command(async () => await GetSongsAsync());
            this.webScraper = webScraper;
            Title = "Updating...";
            ScrollIndex = 0;
            TodayDate = DateOnly.FromDateTime(DateTime.Now);
        }


        async Task GetSongsAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                
                var SongList = await webScraper.GetItems(TodayDate);

                if (Songs.Count != 0)
                {
                    Songs.Clear();
                    SongsNoFilter.Clear();
                }

                Title = TodayDate.ToShortDateString();

                var pinnedItems = await SecureStorage.Default.GetAsync("pinned_items");
                List<pinnedObject> pinnedJson = JsonConvert.DeserializeObject<List<pinnedObject>>("[{\"Name\": \"default\", \"Details\": \"Base\"}]");
                if (pinnedItems.Length > 3)
                {
                    pinnedJson = JsonConvert.DeserializeObject<List<pinnedObject>>(pinnedItems);
                }


                // remove that first irrelevant item 
                foreach (var Song in SongList.GetRange(1, SongList.Count - 1))
                {
                    bool itemExists = pinnedJson.Any(existing => existing.Name == Song.Name
                            && existing.Details == Song.Details);
                    if (itemExists)
                    {
                        Songs.Insert(0, Song);
                        SongsNoFilter.Insert(0, Song);
                    }
                    else
                    {
                        Songs.Add(Song);
                        SongsNoFilter.Add(Song);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }

        }
    }
}
