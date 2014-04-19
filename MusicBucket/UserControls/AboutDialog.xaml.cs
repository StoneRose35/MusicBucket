using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicBucket.UserControls
{
    /// <summary>
    /// Interaktionslogik für AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        public AboutDialog(Assembly assemblyToDisplay):this()
        {
            int build,rev;
            build=assemblyToDisplay.GetName().Version.Build;
            rev=assemblyToDisplay.GetName().Version.Revision;
            DateTime buildDate = new DateTime(2000, 1, 1);
            buildDate = buildDate.AddDays(build);
            buildDate = buildDate.AddSeconds( rev*2 );

            txtbxContent.Text = String.Format(Properties.Resources.AboutDialogText, assemblyToDisplay.GetName().Version, buildDate.ToLongDateString() + ", " + buildDate.ToLongTimeString());

        }
    }
}
