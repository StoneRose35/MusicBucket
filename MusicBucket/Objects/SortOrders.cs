using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBucket.Objects
{
    public class SortOrders
    {
        private const string _filename = "Filename";
        private const string _tracknr = "Tags[0].TrackNumber";
        private const string _trackTitle = "Tags[0].Title";
        private const string _artist = "Tags[0].Artist";
        private const string _album = "Tags[0].Album";
        private const string _tagtypes = "TagsAsString";
        private string[] _SOfilename = new string[] { _filename };
        private string[] _SOtracknr = new string[] { _tracknr,_artist,_album};
        private string[] _SOtrackTitle = new string[] { _trackTitle,_album,_artist };
        private string[] _SOartist = new string[] { _artist,_album,_tracknr };
        private string[] _SOalbum = new string[] { _album,_tracknr };
        private string[] _SOtagtypes = new string[] { _tagtypes, _album, _tracknr, _artist};
        
        public string[] this[string index]
        {
            get
            {
                if(index==_filename)
                {
                    return _SOfilename;
                }
                else if(index==_tracknr)
                {
                    return _SOtracknr;
                }
                else if (index == _trackTitle)
                {
                    return _SOtrackTitle;
                }
                else if (index == _artist)
                {
                    return _SOartist;
                }
                else if (index == _album)
                {
                    return _SOalbum;
                }
                else if (index == _tagtypes)
                {
                    return _SOtagtypes;
                }
                else
                {
                    return null;
                }
            }

        }
    }
}
