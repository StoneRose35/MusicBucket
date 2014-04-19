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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicBucket
{
    /// <summary>
    /// Interaktionslogik für MessageDisplay.xaml
    /// </summary>
    public partial class MessageDisplay : UserControl
    {

        public MessageDisplay()
        {
            InitializeComponent();
            this.labelDisplay.Content = null;
        }

        public void DisplayErrorMessage(string msg,bool noAnim=false)
        {
            Color clr = (Color)this.FindResource("errorColor");
            this.labelDisplay.Foreground = new SolidColorBrush(clr);
            DisplayMessage(msg,noAnim);
        }

        public void DisplayWarningMessage(string msg,bool noAnim=false)
        {
            Color clr = (Color)this.FindResource("warningColor");
            this.labelDisplay.Foreground = new SolidColorBrush(clr);
            DisplayMessage(msg,noAnim);
        }

        public void DisplayInfoMessage(string msg,bool noAnim=false)
        {
            Color clr = (Color)this.FindResource("infoColor");
            this.labelDisplay.Foreground = new SolidColorBrush(clr);
            DisplayMessage(msg,noAnim);
        }
        private void DisplayMessage(string msg,bool noAnim=false)
        {

            Storyboard stbd = (Storyboard)this.FindResource("AnimStoryBoard");
            stbd.Completed += stbd_Completed;
            this.labelDisplay.Content = msg;
            if (!noAnim)
            {
                this.labelDisplay.BeginStoryboard(stbd);
            }
            else
            {
                labelDisplay.Opacity = 1.0;
                (labelDisplay.Effect as System.Windows.Media.Effects.BlurEffect).Radius = 0.0;
            }
        }

        void stbd_Completed(object sender, EventArgs e)
        {
            labelDisplay.Content = "";
        }
    }
}
