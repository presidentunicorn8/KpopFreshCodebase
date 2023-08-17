using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KpopFresh.Model
{
    public class Song
    {
        // constructor
        public Song(string name, string artist, string details, string imageUrl, string songLink, string viewCount)
        {
            this.Name = name;
            this.Artist = artist; 
            this.Details = details;
            this.ImageUrl = imageUrl;
            this.SongLink = songLink;
            this.ViewCount = viewCount; 
        }

        public string Name { get; }
        public string Artist { get;  }
        public string Details { get; }
        public string SongLink { get;  }
        public string ImageUrl { get; }

        public string ViewCount { get; }

    }
}
