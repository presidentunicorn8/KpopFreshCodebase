using KpopFresh.Model;
using KpopFresh.ViewModel;
using KpopFresh.Services;
using CommunityToolkit.Maui.Core.Views;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.Tracing;
using Microsoft.Maui.Graphics;
using System.Globalization;
using Microsoft.Maui.Controls.Internals;

namespace KpopFresh;

class pinnedObject
{ 
    public string Name { get; set; }
    public string Details { get; set; }

}

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class MainPage : ContentPage
{
    public MainPage(SongsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // opening the youtube links in browser
    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var item = ((VisualElement)sender).BindingContext as Song;
        if (item == null || item.SongLink == "no")
        {
            return;
        }

        try
        {
            Uri youtubelink = new(item.SongLink);
            await Browser.Default.OpenAsync(youtubelink, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error Opening Browser", ex.Message, "OK");
        }

    }

    private async void pinItemToTop(object sender, EventArgs e)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            // item swiped on as Song object
            var item = ((SwipeItem)sender).BindingContext as Song;

            ((SwipeItem)sender).BackgroundColor = Colors.Purple;
            SwipeItem swipeItem = (SwipeItem)sender;
            



            // get the songs on the page currently
            ObservableCollection<Song> Songs = ((SongsViewModel)(BindingContext)).Songs;
            // get any possible local storage
            var pinnedItems = await SecureStorage.Default.GetAsync("pinned_items");
            var pinnedString = "";
            
            List<pinnedObject> pinnedJson = JsonConvert.DeserializeObject<List<pinnedObject>>("[{\"Name\": \"default\", \"Details\": \"Base\"}]");

            if (pinnedItems.Length > 3)
            {
                    // remove old months so we have room 
                    pinnedJson = JsonConvert.DeserializeObject<List<pinnedObject>>(pinnedItems);

                    pinnedJson.RemoveAll(existing => DateTime.ParseExact(existing.Name, "MMMM d, yyyy", CultureInfo.CurrentCulture).Month ==
                                                DateTime.Today.AddMonths(-2).Month); 
                    // check if the song already pinned
                    bool itemExists = pinnedJson.Any(existing => existing.Name == item.Name.ToString()
                        && existing.Details == item.Details.ToString());

                if (!itemExists)
                {
                    // if not pinned, add it
                    pinnedJson.Add(new pinnedObject { Name = item.Name, Details = item.Details });
                    pinnedString = JsonConvert.SerializeObject(pinnedJson, Formatting.Indented);
                }
                else
                {
                    pinnedJson.Remove(pinnedJson.Find(existing => existing.Name == item.Name.ToString()
                        && existing.Details == item.Details.ToString()));

                    pinnedString = JsonConvert.SerializeObject(pinnedJson, Formatting.Indented);
                }
            }
            else // we didn't get items from local storage, just add our item without comparing
            {
                pinnedObject newest_object = new pinnedObject { Name = item.Name, Details = item.Details }; 
                List<pinnedObject> pinnedList = new List<pinnedObject>();
                pinnedList.Add(newest_object);
                pinnedString = JsonConvert.SerializeObject(pinnedList, Formatting.None);
            }
            
            await SecureStorage.Default.SetAsync("pinned_items", pinnedString);

            
            // if no songs, get the songs
            if (Songs.Count == 0)
            {
                if ((((SongsViewModel)(BindingContext)) != null) && (((SongsViewModel)(BindingContext)).GetSongsCommand.CanExecute(null)))
                {
                    ((SongsViewModel)(BindingContext)).GetSongsCommand.Execute(null);
                    Songs = ((SongsViewModel)(BindingContext)).Songs;
                }
            }


            // new list for the sorting
            List<Song> sortedSongs = new List<Song>();
            // populate the list from the collection and empty collection
            foreach (Song into in Songs)
            {
                bool itemExists = pinnedJson.Any(existing => existing.Name == into.Name
                        && existing.Details == into.Details);
                if (itemExists)
                {
                    sortedSongs.Insert(0, into); 
                }
                else
                {
                    sortedSongs.Add(into);
                }
                
            }
            Songs.Clear();

            foreach (Song y in sortedSongs)
            {

                    Songs.Add(y);
            }
            sortedSongs.Clear(); 
            
        }
        // once the items are right, control the scroll position.
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }

        return;

    }

    int GetNumber(string viewcount)
    {
        int val = 0;
        bool tryval = int.TryParse(Regex.Match(viewcount, @"\d+", RegexOptions.RightToLeft).Value, out val);

        if (viewcount.Contains('M'))
        {
            val *= 1000000;
        }
        else if (viewcount.Contains('K'))
        {
            val *= 1000;
        }

        return val;
    }

    private async void Sort_Clicked(object sender, EventArgs e)
    {
        if (IsBusy)
            return;

        try
        {
            //public ObservableCollection<Song> Songs { get; set; } = new();
            ObservableCollection<Song> Songs = ((SongsViewModel)(BindingContext)).Songs;
            IsBusy = true;
            int scrollIndex = 0; 

            // ask the user how to sort
            string action = await Shell.Current.DisplayActionSheet("Items Sorting", "Cancel", null, "Normal", "Today", "Reverse", "Popularity");
            if (action == "Cancel") { return; }

            // if no songs, get the songs
            if (Songs.Count == 0)
            {
                if ((((SongsViewModel)(BindingContext)) != null) && (((SongsViewModel)(BindingContext)).GetSongsCommand.CanExecute(null)))
                {
                    ((SongsViewModel)(BindingContext)).GetSongsCommand.Execute(null);
                    Songs = ((SongsViewModel)(BindingContext)).Songs;
                }
            }


            // new list for the sorting
            List<Song> sortedSongs = new List<Song>();
            // populate the list from the collection and empty collection
            foreach (Song item in Songs)
            {
                sortedSongs.Add(item);
            }
            Songs.Clear();
            // preliminary sort to reset
            sortedSongs = sortedSongs.OrderBy(i => DateTime.Parse(i.Name)).ToList();

            // sorting based on action, do nothing for normal sort, it's already done. 
            if (action == "Reverse")
            {
                sortedSongs.Reverse();
            }
            else if (action == "Popularity")
            {
                sortedSongs = sortedSongs.OrderByDescending(v => GetNumber(v.ViewCount)).ThenBy(x => x.Name).ToList();

            }
            else if (action == "Today")  
            {
                DateOnly todayDate = ((SongsViewModel)(BindingContext)).TodayDate;
                scrollIndex = sortedSongs.FindIndex(a => a.Name.Contains(todayDate.ToString("dd")));
                // if there was no song today, try yesterday?
                int i = 1; 
                for (int j = 0; j < todayDate.Day; j++)
                {
                    if (scrollIndex > 1) { break; }
                    scrollIndex = sortedSongs.FindIndex(a => a.Name.Contains((todayDate.Day-i).ToString()));
                }
            }

            // keep pinned items up top no matter what
            var pinnedItems = await SecureStorage.Default.GetAsync("pinned_items");
            List<pinnedObject> pinnedJson = JsonConvert.DeserializeObject<List<pinnedObject>>("[{\"Name\": \"default\", \"Details\": \"Base\"}]");
            if (pinnedItems.Length > 3)
            {
                pinnedJson = JsonConvert.DeserializeObject<List<pinnedObject>>(pinnedItems);
            }

            // list back to collection again
            foreach (Song item in sortedSongs)
            {
                bool itemExists = pinnedJson.Any(existing => existing.Name == item.Name
                       && existing.Details == item.Details);
                if (itemExists)
                {
                    Songs.Insert(0, item); 
                }
                else
                {
                    Songs.Add(item);
                }
                
            }
            // once the items are right, control the scroll position.
            collectionView.ScrollTo(scrollIndex, animate: false); 

        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }

        return;
    }

    private async void Date_Clicked(object sender, EventArgs e)
    {
        if (IsBusy)
            return;

        try
        {
            int ff = 0;
            ff = ff + 1; 
            //public ObservableCollection<Song> Songs { get; set; } = new();
            //ObservableCollection<Song> Songs = ((SongsViewModel)(BindingContext)).Songs;
            IsBusy = true;

            List<DateTime> DateOptions = new List<DateTime>();

            for (int d = -1; d <= 1; d++)
            {
                DateOptions.Add(DateTime.Now.AddMonths(d));
            }
            DateOptions.Reverse();

            // ask the user how to sort
            string action = await Shell.Current.DisplayActionSheet("Month Selection", "Cancel", null, DateOptions[0].ToString("MMMM yyyy"), 
                                                                    DateOptions[1].ToString("MMMM yyyy"), DateOptions[2].ToString("MMMM yyyy"));
            if (action == "Cancel") { return; }

            foreach (DateTime d in DateOptions)
            {
                if (action == d.ToString("MMMM yyyy"))
                {
                    ((SongsViewModel)(BindingContext)).TodayDate = DateOnly.FromDateTime(d);
                    ((SongsViewModel)(BindingContext)).GetSongsCommand.Execute(null);
                    return; 
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

        return;

    }

    public static bool contains(string source, string toCheck)
    {
        return source?.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private async void Filter_Clicked(object sender, EventArgs e)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            //Get songs and make a copy;
            ObservableCollection<Song> Songs = ((SongsViewModel)(BindingContext)).Songs;
            ObservableCollection<Song> SongsNoFilter = ((SongsViewModel)(BindingContext)).SongsNoFilter;
            // new list for the sorting
            List<Song> sortedSongs = new List<Song>();
            

            // ask the user for search term
            string result = await DisplayPromptAsync("Items Filtering", "Enter Search Filter", "OK", "Clear Filters");

            // if no songs, get the songs
            if (Songs.Count == 0)
            {
                if ((((SongsViewModel)(BindingContext)) != null) && (((SongsViewModel)(BindingContext)).GetSongsCommand.CanExecute(null)))
                {
                    ((SongsViewModel)(BindingContext)).GetSongsCommand.Execute(null);
                    Songs = ((SongsViewModel)(BindingContext)).Songs;
                }
            }

            // Cancel operation
            if (result == null)
            {
                // start fresh every time we filter
                foreach (Song item in SongsNoFilter)
                    sortedSongs.Add(item);

                Songs.Clear();
                foreach (Song item in sortedSongs)
                {
                    Songs.Add(item);
                }
                IsBusy = false;
                return;
            }
            else
            {

                if (result.Contains("<") || (result.Contains(">")))
                {
                    int viewfilter = 0;
                    string viewfilterstring = Regex.Match(result, @"\d+").Value;
                    viewfilter = int.Parse(viewfilterstring);
                    if (result.Contains("<"))
                    {
                        foreach (Song item in SongsNoFilter)
                        {
                            if (GetNumber(item.ViewCount) < viewfilter)
                            {
                                sortedSongs.Add(item);
                            }
                        }
                    }
                    else if (result.Contains(">"))
                    {

                        foreach (Song item in SongsNoFilter)
                        {
                            if (GetNumber(item.ViewCount) > viewfilter)
                            {
                                sortedSongs.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    foreach (Song item in SongsNoFilter)
                    {
                        if (contains(item.Name, result) || contains(item.Artist, result) || contains(item.Details, result))
                        {
                            sortedSongs.Add(item);
                        }

                    }
                }
                // if we couldn't find anything to match the term
                if (sortedSongs.Count == 0)
                {
                    bool nosongs = await Shell.Current.DisplayAlert("No match", $"There are no results for your search term: {result}. Please clear filters and try again.", "OK", "Clear Filters");
                    if (nosongs == false)
                    {
                        // populate the list from the collection and empty collection
                        foreach (Song item in SongsNoFilter)
                            sortedSongs.Add(item);

                        Songs.Clear();
                        foreach (Song item in sortedSongs)
                        {
                            Songs.Add(item);
                        }
                    }
                    IsBusy = false;
                    return;
                }
                // re-populate Songs Collection with the fitlered results
                else
                {
                    Songs.Clear();
                    foreach (Song item in sortedSongs)
                    {
                        Songs.Add(item);
                    }
                }
                
                // once the items are right, control the scroll position.
                collectionView.ScrollTo(0, animate: false);
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

        return;

    }

}