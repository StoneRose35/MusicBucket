using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MP3Tagger.Classes;
using PTCAccess;
namespace MusicBucketLib
{
    public class Mp3File
    {
        private List<ID3Tag> _tags;
        private string _fullpath;
        private string _filename;
        private PTCFile _mdfile;
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

        public string TagsAsString
        {
            get
            {
                string res = "";
                if (_tags != null)
                {
                    foreach (ID3Tag tg in _tags)
                    {
                        if (tg is ID3v1)
                        {
                            res += "ID3v1, ";
                        }
                        else if (tg is ID3v22)
                        {
                            res += "ID3v22, ";
                        }
                        else if (tg is ID3v23)
                        {
                            res += "ID3v23, ";
                        }
                    }
                    res = res.TrimEnd(' ').TrimEnd(',');
                }
                else
                {
                    res = "-";
                }
                return res;
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

        public PTCFile MobileDeviceFile
        {
            get
            {
                return _mdfile;
            }
            set
            {
                _mdfile = value;
            }
        }
    }
}
