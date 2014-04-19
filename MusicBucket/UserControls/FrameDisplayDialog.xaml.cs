using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MP3Tagger.Classes;
namespace MusicBucket.UserControls
{
    /// <summary>
    /// Interaktionslogik für FrameDisplayDialog.xaml
    /// </summary>
    public partial class FrameDisplayDialog : Window
    {
        public FrameDisplayDialog()
        {
            InitializeComponent();
        }

        public FrameDisplayDialog(List<TagFrame> tagframes) : this()
        {
            fldisp.ItemsSource = tagframes;
        }
    }
}
