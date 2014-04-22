using System;
using System.Collections.Generic;
using System.IO;
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

namespace PTCAccess
{
    /// <summary>
    /// Interaktionslogik für PTCFolderBrowser.xaml
    /// </summary>
    public partial class PTCFolderBrowser : UserControl
    {
        private object _selectedElement;
        private object _lastElement;
        private PTCWrapper _wrapper;
        public PTCFolderBrowser()
        {
            InitializeComponent();
            _wrapper = new PTCWrapper();
            GetAllRootFolders();
        }


        public object SelectedElement
        {
            get
            {
                return _selectedElement;
            }
        }

        private void lbFolders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IEnumerable<DirectoryInfo> dirs=null;
            List<PTCFolder> mdfolders=null;
            if (_selectedElement != null)
            {
                if (_selectedElement is DriveInfo)
                {
                    dirs = new List<DirectoryInfo>();
                    foreach (DirectoryInfo dinfo in (_selectedElement as DriveInfo).RootDirectory.EnumerateDirectories())
                    {
                        try
                        {
                            dinfo.EnumerateFileSystemInfos();
                            (dirs as List<DirectoryInfo>).Add(dinfo);
                        }
                        catch{}
                    }
                    if (sender != null) // "real" click, is also called artificially
                    {
                        txtbxPath.Text = (_selectedElement as DriveInfo).ToString();
                    }

                }
                else if (_selectedElement is DirectoryInfo)
                {
                    dirs = new List<DirectoryInfo>();
                    foreach (DirectoryInfo dinfo in (_selectedElement as DirectoryInfo).EnumerateDirectories())
                    {
                        try
                        {
                            dinfo.EnumerateFileSystemInfos();
                            (dirs as List<DirectoryInfo>).Add(dinfo);
                        }
                        catch { }
                    }
                    if (sender != null) // "real" click, is also called artificially
                    {
                        txtbxPath.Text += (_selectedElement as DirectoryInfo).ToString() + "\\";
                    }
                }
                else if (_selectedElement is PTCDevice)
                {
                    mdfolders = PTCWrapper.GetFolders(null, (_selectedElement as PTCDevice).ID);
                    if (sender != null) // "real" click, is also called artificially
                    {
                        txtbxPath.Text = "[" + (_selectedElement as PTCDevice).Name + "]:\\";
                    }
                }
                else if (_selectedElement is PTCFolder)
                {
                    mdfolders = PTCWrapper.GetFolders((_selectedElement as PTCFolder).Id, (_selectedElement as PTCFolder).DeviceID);
                    if (sender != null) // "real" click, is also called artificially
                    {
                        txtbxPath.Text += (_selectedElement as PTCFolder).Name + "\\";
                    }
                }

                if(dirs!=null || mdfolders!=null)
                {
                    lbFolders.Items.Clear();
                }
                
                if (dirs != null)
                {
                    foreach (DirectoryInfo dirinfo in dirs)
                    {
                        lbFolders.Items.Add(dirinfo);
                    }
                }
                if (mdfolders != null)
                {
                    foreach (PTCFolder mdfolder in mdfolders)
                    {
                        lbFolders.Items.Add(mdfolder);
                    }
                }
                _lastElement = _selectedElement;
            }
        }

        private void GetAllRootFolders()
        {
            DriveInfo[] drives;
            List<PTCDevice> devices;
            // get all "normal" drives
            drives = DriveInfo.GetDrives();
            foreach(DriveInfo drinfo in drives)
            {
                lbFolders.Items.Add(drinfo);
            }

            //get mobile devices
            devices = PTCWrapper.GetDevices();
            foreach (PTCDevice dev in devices)
            {
                lbFolders.Items.Add(dev);
            }
        }

        private void lbFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbFolders.SelectedItem != null)
            {
                _selectedElement = lbFolders.SelectedItem;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_lastElement is DriveInfo)
            {
                txtbxPath.Text = "";
                _lastElement = null;
            }
            else if (_lastElement is DirectoryInfo)
            {
                txtbxPath.Text = RemoveLastFolderFromPath(txtbxPath.Text);
                _lastElement = (_lastElement as DirectoryInfo).Parent;
            }
            else if (_selectedElement is PTCDevice)
            {
                txtbxPath.Text = "";
                _lastElement = null;
            }
            else if (_selectedElement is PTCFolder)
            {
                txtbxPath.Text = RemoveLastFolderFromPath(txtbxPath.Text);
                _lastElement = (_selectedElement as PTCFolder).Parent;
            }
            if (_lastElement == null)
            {
                lbFolders.Items.Clear();
                GetAllRootFolders();
            }
            else
            {
                _selectedElement = _lastElement;
                lbFolders_MouseDoubleClick(null, null);
            }
        }

        private string RemoveLastFolderFromPath(string path)
        {
            string[] splitted;
            string res = "";
            splitted = path.TrimEnd('\\').Split('\\');
            for (int k = 0; k < splitted.Length - 1; k++)
            {
                res += splitted[k] + "\\";
            }
            return res;
        }
    }
}
