using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicBucket.UserControls
{
    /// <summary>
    /// Interaktionslogik für NewBucketDialog.xaml
    /// </summary>
    public partial class NewBucketDialog : Window
    {
        private string _Path, _Title;
        private bool _includeSubFolders;
        public NewBucketDialog()
        {
            InitializeComponent();
        }

        public string Path
        {
            get
            {
                return _Path;
            }
        }

        public new string Title
        {
            get
            {
                return _Title;
            }
        }

        public bool IncludeSubFolder
        {
            get
            {
                return _includeSubFolders;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbDlg = new FolderBrowserDialog();
            fbDlg.Owner = this;
            if (fbDlg.ShowDialog().Value)
            {
                _Path = fbDlg.SelectedPath;
                textBoxPath.Text = _Path;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
            this._Path = "";
            this._Title = "";
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this._Title = textboxTitle.Text;
            this._includeSubFolders = checkboxIncludeSF.IsChecked.Value;
            this._Path = textBoxPath.Text;
            this.Close();
        }
    }
}
