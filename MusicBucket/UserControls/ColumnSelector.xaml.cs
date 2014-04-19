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

namespace MusicBucket.UserControls
{
    /// <summary>
    /// Interaktionslogik für ColumnSelector.xaml
    /// </summary>
    public partial class ColumnSelector : Window
    {

        private string _selectedValue = "";
        public ColumnSelector()
        {
            InitializeComponent();
        }

        public ColumnSelector(IEnumerable<string> values)
            : this()
        {
            columnSelector.ItemsSource = values;
        }

        public string SelectedValue
        {
            get
            {
                return _selectedValue;
            }
        }

        private void columnSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedValue = columnSelector.SelectedValue.ToString();
            this.Close();
        }
    }
}
