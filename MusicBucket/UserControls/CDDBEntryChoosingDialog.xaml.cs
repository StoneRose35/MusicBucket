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

using CDDBAccess;
namespace MusicBucket
{
    /// <summary>
    /// Interaktionslogik für CDDBEntryChoosingDialog.xaml
    /// </summary>
    public partial class CDDBEntryChoosingDialog : Window
    {
        private CDDBEntry _selectedEntry;
        public CDDBEntryChoosingDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the data to display, never hand over an emtpy list or null
        /// </summary>
        /// <param name="entries"></param>
        public void SetData(IEnumerable<CDDBEntry> entries)
        {
            this.entriesGrid.ItemsSource = entries;
            this.entriesGrid.SelectedIndex = 0;
        }

        public CDDBEntry SelectedEntry
        {
            get
            {
                return _selectedEntry;
            }
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void entriesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedEntry = this.entriesGrid.SelectedItem as CDDBEntry;
        }
    }
}
