using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBucket.Objects
{
    public class ImportProgressInfo
    {
        private int _progress;
        private string _declaration;
        private int _tracknumber;

        public ImportProgressInfo(int progress, string declaration, int tracknumber)
        {
            _progress = progress;
            _declaration = declaration;
            _tracknumber = tracknumber;
        }

        public int Progress
        {
            get { return _progress; }
        }

        public string Declaration
        {
            get { return _declaration; }
        }

        public int TrackNumber
        {
            get { return _tracknumber; }
        }
    }
}
