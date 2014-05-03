using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBucketLib;
using System.Collections.ObjectModel;
namespace MusicBucket.Objects
{
    /// <summary>
    /// Settings for the entire application, is entirely serialized to a known location (%APPDATA%/MusicBucket)
    /// </summary>
    [Serializable]
    public class UserSettings
    {
        private ObservableCollection<Bucket> _buckets;
        private MusicBucketImportSettings _importSettings;
        private GridWidthAnimation _visuals;
        private string _locale;
        public UserSettings()
        {
            _buckets = new ObservableCollection<Bucket>();
            _importSettings = new MusicBucketImportSettings();
            _visuals = new GridWidthAnimation();
        }

        public ObservableCollection<Bucket> Buckets
        {
            get { return _buckets; }
            set { _buckets = value; }
        }

        public MusicBucketImportSettings ImportSettings
        {
            get { return _importSettings; }
            set { _importSettings = value; }
        }

        public GridWidthAnimation Visuals
        {
            get
            {
                return _visuals;
            }
            set
            {
                _visuals = value;
            }
        }

        /// <summary>
        /// Gets the application locale currently set, must me a locale-compatible string, such as for example fr-FR
        /// </summary>
        public string ApplicationLocale
        {
            get
            {
                return _locale;
            }
            set
            {
                _locale = value;
            }
        }
    }
}
