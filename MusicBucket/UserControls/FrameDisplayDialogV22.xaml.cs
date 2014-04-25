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
using System.Windows.Shapes;

using MP3Tagger.Classes;
namespace MusicBucket.UserControls
{
    /// <summary>
    /// Interaktionslogik für FrameDisplayDialogV22.xaml
    /// </summary>
    public partial class FrameDisplayDialogV22 : Window
    {
        public FrameDisplayDialogV22()
        {
            InitializeComponent();
        }

        public FrameDisplayDialogV22(List<TagFrameV22> tagframes)
            : this()
        {
            fldisp.ItemsSource = tagframes;
        }
    }
}
