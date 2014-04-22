using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MP3Tagger.Classes;

namespace MusicBucketLib
{
    public class Mp3File
    {
        private List<ID3Tag> _tags;
        private string _fullpath;
        private string _filename;

        public List<ID3Tag> Tags
        {
            get
            {
                return _tags;
            }
            set
            {
                _tags = value;
            }
        }

        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
            }
        }

        public string FullPath
        {
            get
            {
                return _fullpath;
            }
            set
            {
                _fullpath = value;
            }
        }
    }
}
