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
using MusicBucket.Objects;
namespace MusicBucket.UserControls
{
    /// <summary>
    /// Interaktionslogik für ImportSettingsDialog.xaml
    /// </summary>
    public partial class ImportSettingsDialog : Window
    {
        private MusicBucketImportSettings _backup;
        private bool _commitChanges = false;
        public ImportSettingsDialog()
        {
            InitializeComponent();
            cmbBitRates.Items.Add((int)128);
            cmbBitRates.Items.Add((int)160);
            cmbBitRates.Items.Add((int)192);
            this.DataContextChanged += ImportSettingsDialog_DataContextChanged;
        }

        void ImportSettingsDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _backup = ((MusicBucketImportSettings)this.DataContext).Clone();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            _commitChanges = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_commitChanges)
            {
                this.DataContext = _backup.Clone();
            }
        }
        

    }
}
