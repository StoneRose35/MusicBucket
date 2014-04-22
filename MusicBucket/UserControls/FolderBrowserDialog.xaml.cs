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

namespace MusicBucket.UserControls
{
    /// <summary>
    /// Interaktionslogik für FolderBrowserDialog.xaml
    /// </summary>
    public partial class FolderBrowserDialog : Window
    {
        private string _path;
        public FolderBrowserDialog()
        {
            InitializeComponent();
        }

        public string SelectedPath
        {
            get
            {
                return _path;
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            _path = fbrowser.Path;
            DialogResult = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            _path = null;
            DialogResult = false;
            Close();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
               // DialogResult = false;
            }
        }
    }
}
