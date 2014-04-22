using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MusicBucketLib;
namespace MusicBucket.Converters
{
    public class BucketTitleConverter : IValueConverter
    {
        /// <summary>
        /// first argument: bucket title (string),
        /// second argument: bucket path (string),
        /// third argument: include subfolders (bool)
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            else
            {
                return String.Format("{0}: {1}\r\n{2}: {3},{4}", Properties.Resources.BucketDescriptionTitlePrefix, (value as Bucket).Title, Properties.Resources.BucketDescriptionPathPrefix, (value as Bucket).Path, (value as Bucket).IncludeSubFolders ? Properties.Resources.BucketDescriptionIncludingSubfolders : Properties.Resources.BucketDescriptionExcludingSubfolders);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
