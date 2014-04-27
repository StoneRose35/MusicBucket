using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using MP3Tagger.Classes;
using CDRipperLib;
using CDDBAccess;
using Yeti.MMedia.Mp3;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MusicBucket.UserControls;
using MusicBucket.Objects;
using MusicBucketLib;
using System.Windows.Threading;
namespace MusicBucket
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged
    {
        private const double _GRIDANIMDURATION = 2.34;
        private const double _MINIMALWIDTH = 20;
        private string _STORAGEPATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\MusicBucket";
        private const string _STORAGEFILE = "Settings.lst";
        private const string _TEMPWAVENAME = "Obsidian.wav";
        private const string _TEMPMP3NAME = "WhiteWash.mp3";
        private const string _MP3CONFIGFILE = "LameConfig.bin";
        private const string _FILENAMETEMPLATE = "{0} - {1}.mp3";
        private const string _DRAGCONTENTTYPE = "MusicBucketPlayerList";

        private List<ID3Tag> _tags;
        private ObservableCollection<ID3Tag> _otags;

        private UserSettings _userSettings;

        private Bucket _currentBucket;
        private ObservableCollection<Mp3File> _mp3s;
        private ObservableCollection<Mp3File> _playerQueue;
        private GridLength _iWidth, _bWidth, _pWidth;
        private Storyboard _stbd;
        private int _importStage;

        private BackgroundWorker _importWorker;
        private BackgroundWorker _readBucketWorker;
        private List<Bucket> _selectedBucketsForImport;
        private string _selectedCDDrive;
        private System.Drawing.Image _copyImg;
        private GridViewColumnHeader _lastHeaderClicked;
        private ListSortDirection _lastDirection;
        private bool _startImportAfterCancellingReadingBucketContent=false;
        private bool _dragdropallowed,_dragdropplayerallowed;
        private bool _playerIsPlaying,_playerIsPaused;
        private DispatcherTimer _timer;

        #region bound public properties

        public ObservableCollection<Bucket> Buckets
        {
            get
            {
                return _userSettings.Buckets;
            }
            set 
            {
                _userSettings.Buckets = value;
            }
        }

        public ObservableCollection<Mp3File> CurrentMp3s
        {
            get
            {
                if (_mp3s == null)
                {
                    _mp3s = new ObservableCollection<Mp3File>();
                }
                return _mp3s;
            }
            set
            {
                _mp3s = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentMp3s"));
            }
        }

        public ObservableCollection<Mp3File> PlayerQueue
        {
            get
            {
                if (_playerQueue == null)
                {
                    _playerQueue = new ObservableCollection<Mp3File>();
                }
                return _playerQueue;
            }
            set
            {
                _playerQueue = value;
                PropertyChanged(this, new PropertyChangedEventArgs("PlayerQueue"));
            }
        }

        public Bucket CurrentBucket
        {
            get 
            {
                return _currentBucket;
            }
            set
            {
                _currentBucket = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentBucket"));
                }
            }
        }

        public double MINIMALWIDTH
        {
            get
            {
                return _MINIMALWIDTH;
            }
        }

        public double MINIMALWIDTHEXT
        {
            get
            {
                return _MINIMALWIDTH+4;
            }
        }

        public bool PasteImageEnabled
        {
            get
            {
                return System.Windows.Clipboard.ContainsImage();
            }
        }
        #endregion

        public MainWindow()
        {
            DriveInfo[] drives;
            InitializeComponent();
            drives = DriveInfo.GetDrives();
            for(int z=0;z<drives.Length;z++)
            {
                if(drives[z].DriveType==DriveType.CDRom)
                {
                    cmbCDDrives.Items.Add(drives[z]);
                }
            }
            // select first cd drive found by default, may be overridden by session setting read at a later stage
            if (cmbCDDrives.Items.Count > 0)
            {
                cmbCDDrives.SelectedIndex = 0;
            }
            else
            {
                buttonStartImport.IsEnabled = false;
            }
            LoadBuckets();
            bucketDisp.ItemsSource = _userSettings.Buckets;
            _otags = new ObservableCollection<ID3Tag>();
            this.maingrid.DataContext = this;
            _importStage = 0;

            

            _importWorker = new BackgroundWorker();
            _importWorker.WorkerReportsProgress = true;
            _importWorker.RunWorkerCompleted += _importWorker_RunWorkerCompleted;
            _importWorker.DoWork += _importWorker_DoWork;
            _importWorker.ProgressChanged += _importWorker_ProgressChanged;

            _readBucketWorker = new BackgroundWorker();
            _readBucketWorker.WorkerReportsProgress = true;
            _readBucketWorker.WorkerSupportsCancellation = true;
            _readBucketWorker.DoWork += _readBucketWorker_DoWork;
            _readBucketWorker.ProgressChanged += _readBucketWorker_ProgressChanged;
            _readBucketWorker.RunWorkerCompleted += _readBucketWorker_RunWorkerCompleted;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        #region read buckets

        void _readBucketWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                {
                    CurrentMp3s = null;
                    if (_startImportAfterCancellingReadingBucketContent)
                    {
                        _importWorker.RunWorkerAsync();
                    }
                    msgDisp.DisplayInfoMessage("");
                }
                CurrentBucket.Mp3FileRead -= b_Mp3FileRead;
            }
            catch { }
        }

        void _readBucketWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                msgDisp.DisplayInfoMessage("", true);
                _mp3s.Add(e.UserState as Mp3File);
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentMp3s"));
            }
            else if (e.ProgressPercentage == 2)
            {
                msgDisp.DisplayInfoMessage((string)e.UserState, true);
            }
            else
            {
                msgDisp.DisplayInfoMessage((string)e.UserState);
            }
        }

        void _readBucketWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (CurrentBucket != null)
            {
                CurrentBucket.Mp3FileRead += b_Mp3FileRead;
                CurrentBucket.GetMp3Files(_readBucketWorker,e);
            }
        }

        void b_Mp3FileRead(Mp3File file)
        {
            _readBucketWorker.ReportProgress(1, file);
        }

        #endregion

        #region import functionality
        void _importWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                msgDisp.DisplayInfoMessage((string)(e.UserState as Objects.ImportProgressInfo).Declaration + (e.UserState as Objects.ImportProgressInfo).TrackNumber,true);
                progressImport.Value = (double)(e.UserState as Objects.ImportProgressInfo).Progress;
            }
            else if (e.ProgressPercentage == 2)
            {
                msgDisp.DisplayWarningMessage((string)e.UserState);
            }
        }

        void _importWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                msgDisp.DisplayInfoMessage(Properties.Resources.importTerminated);
                _importStage = 0;
                buttonStartImport.Content = Properties.Resources.startImportText;
                progressImport.Value = 0;
            }
            catch { }
        }

        void _importWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int trackcnt = 0;
            int read;
            long wavesize, totalread;
            Stream inStream, outStream;

            Mp3Writer mp3writer;
            string filename;
            byte[] copybfr;
            CDROM_TOC toc;
            WaveWriter wavewriter = new WaveWriter();
            CDRipper cdr = new CDRipper(_selectedCDDrive);
            toc = cdr.READ_TOC();
            foreach (ID3Tag tag in _otags)
            {
                if (File.Exists(_STORAGEPATH + "\\" + _TEMPWAVENAME))
                {
                    File.Delete(_STORAGEPATH + "\\" + _TEMPWAVENAME);
                }
                if (File.Exists(_STORAGEPATH + "\\" + _TEMPMP3NAME))
                {
                    File.Delete(_STORAGEPATH + "\\" + _TEMPMP3NAME);
                }
                wavewriter.CreateFile(_STORAGEPATH + "\\" + _TEMPWAVENAME);
                cdr.NotifyProgress += cdr_NotifyProgress;
                cdr.ReadTrack(toc, trackcnt, wavewriter);
                wavewriter.CloseFile();
                inStream = File.Open(_STORAGEPATH + "\\" + _TEMPWAVENAME, FileMode.Open);
                if (tag.Title.Length > 0 && tag.Artist.Length > 0)
                {
                    filename = String.Format(_FILENAMETEMPLATE, tag.Title, tag.Artist);
                }
                else
                {
                    filename = String.Format("mp3_of_{0}_{1}_{2}_{3}_{4}-{5}.mp3", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                }
                outStream = File.Open(_STORAGEPATH + "\\" + _TEMPMP3NAME, FileMode.CreateNew);
                mp3writer = new Mp3Writer(outStream, new Mp3WriterConfig(new WaveLib.WaveFormat(44100, 16, 2), new Yeti.Lame.BE_CONFIG(new WaveLib.WaveFormat(44100, 16, 2), (uint)_userSettings.ImportSettings.BitRate)));
                copybfr = new byte[mp3writer.OptimalBufferSize];
                wavesize = (int)inStream.Length;
                totalread = 0;
                while ((read = inStream.Read(copybfr, 0, mp3writer.OptimalBufferSize)) > 0)
                {
                    totalread += read;
                    mp3writer.Write(copybfr);
                    _importWorker.ReportProgress(1, new Objects.ImportProgressInfo((int)((double)totalread / (double)wavesize * 100.0), Properties.Resources.ProgressConversion, trackcnt+1));
                    //_importWorker.ReportProgress(1, String.Format(Properties.Resources.progressEncodingDisplay, (int)totalread / wavesize * 100.0, trackcnt));
                }
                inStream.Close();
                if (_copyImg != null)
                {
                    (tag as ID3v23).FrontCover = _copyImg;
                }
                (tag as ID3v23).Write(outStream as FileStream);
                outStream.Close();

                foreach (Bucket b in _selectedBucketsForImport)
                {
                    try
                    {
                        if(b.IsOnPortableDevice)
                        {
                            PTCAccess.PTCFolder outfolder;
                            PTCAccess.PTCWrapper.Exists(b.Path,out outfolder);
                            PTCAccess.PTCWrapper.CopyMp3File(_STORAGEPATH + "\\" + _TEMPMP3NAME,filename, outfolder);
                        }
                        else
                        {
                            File.Copy(_STORAGEPATH + "\\" + _TEMPMP3NAME, b.Path + "\\" + filename);
                        }
                        
                    }
                    catch 
                    {
                        _importWorker.ReportProgress(2, String.Format(Properties.Resources.copyToBucketError, b.Path));
                    }
                }
                trackcnt++;
            }
        }





        private void buttonStartImport_Click_1(object sender, RoutedEventArgs e)
        {
            CDDBEntry[] dl;
            CDDBEntry chosenEntry;
            CDDBEntryChoosingDialog entryChooser;
            string extdata;
            //CDDBEntry onesecond;

            CDROM_TOC toc;
            if (_importStage == 0)
            {
                if (cmbCDDrives.SelectedItem != null)
                {
                    CDRipper cdr = new CDRipper(cmbCDDrives.SelectedItem.ToString().Substring(0, 1).ToLower());
                    CDDBConnection cddbconn;
                    cddbconn = new CDDBConnection();
                    toc = cdr.READ_TOC();
                    if (toc != null)
                    {
                        try
                        {
                            dl = cddbconn.QueryCD(toc);
                            if (dl != null)
                            {
                                if (dl.Length > 1)
                                {
                                    entryChooser = new CDDBEntryChoosingDialog();
                                    entryChooser.SetData(dl.AsEnumerable<CDDBEntry>());
                                    entryChooser.ShowDialog();
                                    chosenEntry = entryChooser.SelectedEntry;
                                    if (chosenEntry == null)
                                    {
                                        throw new MusicBucketException("CDDB Entry selection not made (was list empty?)");
                                    }
                                }
                                else
                                {
                                    chosenEntry = dl[0];
                                }
                                _tags = cddbconn.ReadCD(chosenEntry, MP3Tagger.TagTypeEnum.ID3v23, out extdata);
                                _otags.Clear();
                                foreach (ID3Tag tg in _tags)
                                {
                                    _otags.Add(tg);
                                }
                                dgImport.ItemsSource = _otags;
                                _importStage = 1;
                                buttonStartImport.Content = Properties.Resources.continueImportText;
                            }
                            else
                            {
                                msgDisp.DisplayInfoMessage(Properties.Resources.NoCDDBEntriesFound);
                                _tags = new List<ID3Tag>();
                                for (int k = 0; k < toc.LastTrack; k++)
                                {
                                    _tags.Add(new ID3v23() { TrackNumber = k + 1 });
                                }
                                _otags.Clear();
                                foreach (ID3Tag tg in _tags)
                                {
                                    _otags.Add(tg);
                                }
                                dgImport.ItemsSource = _otags;
                                _importStage = 1;
                                buttonStartImport.Content = Properties.Resources.continueImportText;
                            }
                        }
                        catch (CDDBConnectionException exc) // connection to cddb failed
                        {
                            msgDisp.DisplayErrorMessage(exc.Message);
                            _tags = new List<ID3Tag>();
                            for (int k = 0; k < toc.LastTrack; k++)
                            {
                                _tags.Add(new ID3v23() { TrackNumber = k + 1 });
                            }
                            _otags.Clear();
                            foreach (ID3Tag tg in _tags)
                            {
                                _otags.Add(tg);
                            }
                            dgImport.ItemsSource = _otags;
                            _importStage = 1;
                            buttonStartImport.Content = Properties.Resources.continueImportText;
                        }
                        catch (MusicBucketException exc2)
                        {
                            msgDisp.DisplayErrorMessage(exc2.Message);
                        }
                    }
                    else
                    {
                        msgDisp.DisplayWarningMessage(Properties.Resources.NoCDInserted);
                    }
                    dl = cddbconn.QueryCD(toc);
                }
                else
                {
                    msgDisp.DisplayWarningMessage(Properties.Resources.NoCDDriveSelected);
                }
            }
            else if (_importStage == 1) // tracks have been read and user has entered tag info and image
            {
                int attachedbucketsCounter = 0;
                WaveWriter wavewriter = new WaveWriter();
                IList selectedBuckets;
                if (cmbCDDrives.SelectedItem != null)
                {
                    selectedBuckets = bucketDisp.SelectedItems;
                    _selectedBucketsForImport  = new List<Bucket>();
                    foreach (Bucket b1 in selectedBuckets)
                    {
                        if (b1.IsAttached)
                        {
                            attachedbucketsCounter++;
                            _selectedBucketsForImport.Add(b1);
                        }
                    }
                    _selectedCDDrive = cmbCDDrives.SelectedItem.ToString().Substring(0, 1).ToLower();
                    _copyImg = imageframe.Tag as System.Drawing.Image;
                    if(attachedbucketsCounter>0)
                    {
                        if (!_importWorker.IsBusy)
                        {
                            if (_readBucketWorker.IsBusy)
                            {
                                _startImportAfterCancellingReadingBucketContent = true;
                                _readBucketWorker.CancelAsync();
                            }
                            else
                            {
                                _importWorker.RunWorkerAsync();
                            }
                        }
                        else
                        {
                            msgDisp.DisplayWarningMessage(Properties.Resources.importProcessRunning);
                        }
                    }
                    else
                    {
                        msgDisp.DisplayWarningMessage(Properties.Resources.noBucketSelectedText);
                    }
                }
                else
                {
                    msgDisp.DisplayWarningMessage(Properties.Resources.NoCDInserted);
                }

            }


            
        }

        void cdr_NotifyProgress(int tracknumber,int progress)
        {
            _importWorker.ReportProgress(1, new Objects.ImportProgressInfo(progress, Properties.Resources.ProgressImport, tracknumber + 1));
        }

        #endregion

        #region state saving

        private void LoadBuckets()
        {
            try
            {
                FileStream fstream = new FileStream(_STORAGEPATH + "\\" + _STORAGEFILE, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                IFormatter fmtter = new BinaryFormatter();
                _userSettings = (UserSettings)fmtter.Deserialize(fstream);
                fstream.Close();
            }
            catch
            {
                _userSettings = new UserSettings();
                msgDisp.DisplayErrorMessage(Properties.Resources.UserSettingsNotOpened);
            }
        }

        #endregion

        #region visuals

        void stbd_Completed(object sender, EventArgs e)
        {
            colImport.Width = _iWidth;
            colBuckets.Width = _bWidth;
            colPlayer.Width = _pWidth;
        }


        private void CreateStoryboard()
        {
            _stbd = new Storyboard();

            ParallelTimeline ptl = new ParallelTimeline();
            GridWidthAnimation gwaI = new GridWidthAnimation() { Name = "gwaI" };
            GridWidthAnimation gwaB = new GridWidthAnimation() { Name = "gwaB" };
            GridWidthAnimation gwaP = new GridWidthAnimation() { Name = "gwaP" };

            gwaI.Duration = new Duration(TimeSpan.FromSeconds(_GRIDANIMDURATION));
            gwaB.Duration = new Duration(TimeSpan.FromSeconds(_GRIDANIMDURATION));
            gwaP.Duration = new Duration(TimeSpan.FromSeconds(_GRIDANIMDURATION));
            ptl.Children.Add(gwaI);
            ptl.Children.Add(gwaB);
            ptl.Children.Add(gwaP);
            _stbd.Children.Add(ptl);
            Storyboard.SetTargetName(gwaI, "colImport");
            Storyboard.SetTargetProperty(gwaI, new PropertyPath("Width"));
            Storyboard.SetTargetName(gwaB, "colBuckets");
            Storyboard.SetTargetProperty(gwaB, new PropertyPath("Width"));
            Storyboard.SetTargetName(gwaP, "colPlayer");
            Storyboard.SetTargetProperty(gwaP, new PropertyPath("Width"));
            _stbd.Completed += stbd_Completed;
        }

        /// <summary>
        /// Prepares the storyboard to be run
        /// </summary>
        /// <param name="colToExpand">The column to epand, 0 is import, 1 is buckets list, 2 is player</param>
        private void PrepareStoryboard(int colToExpand)
        {
            double actwidth,widthtoset;
            double sharpness, bouncyness;
            int frequency;
            GridLength mgl;
            if (_stbd == null)
            {
                CreateStoryboard();
            }
            actwidth = this.maingrid.ActualWidth;
            if (actwidth > 3*_MINIMALWIDTH)
            {
                widthtoset = actwidth - 2*_MINIMALWIDTH;
            }
            else
            {
                widthtoset = actwidth/3;
            }
            mgl = new GridLength(MINIMALWIDTHEXT, GridUnitType.Star);


            bouncyness = _userSettings.Visuals.Bouncyness;
            sharpness = _userSettings.Visuals.Sharpness;
            frequency = _userSettings.Visuals.Frequency;

            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaI") as GridWidthAnimation).From = colImport.Width;
            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaI") as GridWidthAnimation).Sharpness = sharpness;
            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaI") as GridWidthAnimation).Frequency = frequency;
            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaI") as GridWidthAnimation).Bouncyness = bouncyness;
            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaB") as GridWidthAnimation).From = colBuckets.Width;
            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaB") as GridWidthAnimation).Sharpness = sharpness;
            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaB") as GridWidthAnimation).Frequency = frequency;
            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaB") as GridWidthAnimation).Bouncyness = bouncyness;
            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaP") as GridWidthAnimation).From = colPlayer.Width;
            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaP") as GridWidthAnimation).Sharpness = sharpness;
            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaP") as GridWidthAnimation).Frequency = frequency;
            ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaP") as GridWidthAnimation).Bouncyness = bouncyness;
            switch (colToExpand)
            {
                case 0:
                    ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaI") as GridWidthAnimation).To = new GridLength(widthtoset,GridUnitType.Star);
                    ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaB") as GridWidthAnimation).To = mgl;
                    ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaP") as GridWidthAnimation).To = mgl;
                    break;
                case 1:
                    ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaI") as GridWidthAnimation).To = mgl;
                    ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaB") as GridWidthAnimation).To = new GridLength(widthtoset, GridUnitType.Star);
                    ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaP") as GridWidthAnimation).To = mgl;
                    break;
                case 2:
                    ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaI") as GridWidthAnimation).To = mgl;
                    ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaB") as GridWidthAnimation).To = mgl;
                    ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaP") as GridWidthAnimation).To = new GridLength(widthtoset, GridUnitType.Star);
                    break;
            }
            _iWidth = ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaI") as GridWidthAnimation).To;
            _bWidth = ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaB") as GridWidthAnimation).To;
            _pWidth = ((_stbd.Children[0] as ParallelTimeline).Children.First(tl => tl.Name == "gwaP") as GridWidthAnimation).To;
        }

        #endregion

        #region gui handlers

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonImport_Click_1(object sender, RoutedEventArgs e)
        {
            PrepareStoryboard(0);
            this.BeginStoryboard(_stbd);

        }

        private void ButtonBucket_Click_1(object sender, RoutedEventArgs e)
        {
            PrepareStoryboard(1);
            this.BeginStoryboard(_stbd);

        }

        private void ButtonPlayer_Click_1(object sender, RoutedEventArgs e)
        {
            PrepareStoryboard(2);
            this.BeginStoryboard(_stbd);
        }


        private void Window_ContentRendered_1(object sender, EventArgs e)
        {
            this.maingrid.ColumnDefinitions[0].Width = new GridLength(this.maingrid.ColumnDefinitions[0].ActualWidth, GridUnitType.Star);
            this.maingrid.ColumnDefinitions[1].Width = new GridLength(this.maingrid.ColumnDefinitions[1].ActualWidth, GridUnitType.Star);
            this.maingrid.ColumnDefinitions[2].Width = new GridLength(this.maingrid.ColumnDefinitions[2].ActualWidth, GridUnitType.Star);
        }

        private void buttonAddBucket_Click(object sender, RoutedEventArgs e)
        {
            Bucket b;
            UserControls.NewBucketDialog nbDlg = new UserControls.NewBucketDialog();
            nbDlg.Owner = this;
            nbDlg.ShowDialog();
            if (nbDlg.Path != null)
            {
                if (nbDlg.Path != "")
                {
                    b = new Bucket();
                    b.Path = nbDlg.Path;
                    b.IncludeSubFolders = nbDlg.IncludeSubFolder;
                    b.Title = nbDlg.Title;
                    b.Update();
                     _userSettings.Buckets.Add(b);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_importWorker.IsBusy)
                {
                    System.Windows.MessageBox.Show(Properties.Resources.importRunning);
                    e.Cancel = true;
                    return;
                }
                if (_readBucketWorker.IsBusy)
                {
                    _readBucketWorker.CancelAsync();
                }
                MediaPlayer player = (MediaPlayer)Resources["MPlayer"];
                if (_playerIsPlaying || _playerIsPaused)
                {
                    player.Stop();
                    player.Close();
                }
                if (!Directory.Exists(_STORAGEPATH))
                {
                    Directory.CreateDirectory(_STORAGEPATH);
                }
                FileStream fstream = new FileStream(_STORAGEPATH + "\\" + _STORAGEFILE, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                IFormatter fmtter = new BinaryFormatter();
                fmtter.Serialize(fstream, _userSettings);
                fstream.Close();
            }
            catch
            {
                if (System.Windows.MessageBox.Show(Properties.Resources.UserSettingsNotSaved, Properties.Resources.SettingsNotSavedMsgBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }

            
        }

        private void bucketDisp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Bucket b;
            b = (bucketDisp.SelectedItem as Bucket);
            if(b!=null)
            { 
                CurrentBucket = b;
                if (!_readBucketWorker.IsBusy)
                {
                    _readBucketWorker.RunWorkerAsync();
                }
            }
            else
            {
                if(_readBucketWorker.IsBusy)
                {
                    _readBucketWorker.CancelAsync();
                }
                else
                {
                    CurrentMp3s = null;
                }
                
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Image img;
            img = System.Windows.Forms.Clipboard.GetImage();
            imageframe.Tag = img;
            imageframe.Source = System.Windows.Clipboard.GetImage();
        }


        private void Canvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            dropimage.IsEnabled = this.PasteImageEnabled;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            imageframe.Tag = null;
            _otags.Clear();
            dgImport.ItemsSource = _otags;
            imageframe.Source = (this.Resources["BlankImage"] as Image).Source;
            _importStage = 0;
            buttonStartImport.Content = Properties.Resources.startImportText;
        }



        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (bucketDisp.SelectedItem != null)
            {
                if(System.Windows.MessageBox.Show(Properties.Resources.ReallyRemoveBucket,Properties.Resources.ReallyRemoveBucketTitle,System.Windows.MessageBoxButton.YesNo)==MessageBoxResult.Yes)
                {
                    _userSettings.Buckets.Remove(bucketDisp.SelectedItem as Bucket);
                }
            }
        }

        private void lvFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvFiles.SelectedItem != null)
            {
                buttonShowFrameList.IsEnabled = false;
                if ((lvFiles.SelectedItem as Mp3File).Tags != null)
                {
                    List<ID3Tag> ts = (lvFiles.SelectedItem as Mp3File).Tags;
                    foreach (ID3Tag tg in ts)
                    {
                        if(tg is ID3v23 || tg is ID3v22)
                        {
                            buttonShowFrameList.IsEnabled = true;
                        }
                    }
                }
                else
                {
                    buttonShowFrameList.IsEnabled = false;
                }
            }
            else
            {
                buttonShowFrameList.IsEnabled = false;
            }
        }

        private void buttonShowFrameList_Click(object sender, RoutedEventArgs e)
        {
            UserControls.FrameDisplayDialog fddlg;
            UserControls.FrameDisplayDialogV22 fddlgv22;
            List<ID3Tag> ts = (lvFiles.SelectedItem as Mp3File).Tags;
            foreach (ID3Tag tg in ts)
            {
                if (tg is ID3v23)
                {
                    fddlg = new UserControls.FrameDisplayDialog((tg as ID3v23).Frames);
                    fddlg.Show();
                }
                if (tg is ID3v22)
                {
                    fddlgv22 = new UserControls.FrameDisplayDialogV22((tg as ID3v22).Frames);
                    fddlgv22.Show();
                }
            }

        }

        private void lvFiles_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked =
      e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string header = headerClicked.Column.Header as string;
                    string test = (headerClicked.Column.DisplayMemberBinding as System.Windows.Data.Binding).Path.Path;
                    Sort(test, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }


                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }
        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(lvFiles.ItemsSource);
            lvFiles.Items.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            lvFiles.Items.SortDescriptions.Add(sd);
            lvFiles.Items.Refresh();
        }

        private void dgImport_Click(object sender, RoutedEventArgs e)
        {
            ID3Tag tag;
            IEnumerable<string> distvals=null;
            UserControls.ColumnSelector colSelector;
            if (dgImport.Items.Count > 0)
            {
                DataGridColumnHeader headerClicked = e.OriginalSource as DataGridColumnHeader;

                if (headerClicked.Column == dg_tracknr)
                {

                    distvals = dgImport.Items.OfType<ID3Tag>().Select<ID3Tag, string>(x => x.TrackNumber.ToString()).Distinct();
                }
                else if (headerClicked.Column == dg_title)
                {

                    distvals = dgImport.Items.OfType<ID3Tag>().Select<ID3Tag, string>(x => x.Title).Distinct();
                }
                else if (headerClicked.Column == dg_artist)
                {

                    distvals = dgImport.Items.OfType<ID3Tag>().Select<ID3Tag, string>(x => x.Artist).Distinct();
                }
                else if (headerClicked.Column == dg_album)
                {

                    distvals = dgImport.Items.OfType<ID3Tag>().Select<ID3Tag, string>(x => x.Album).Distinct();
                }
                else if (headerClicked.Column == dg_year)
                {

                    distvals = dgImport.Items.OfType<ID3Tag>().Select<ID3Tag, string>(x => x.Year.ToString()).Distinct();
                }
                else if (headerClicked.Column == dg_genre)
                {

                    distvals = dgImport.Items.OfType<ID3Tag>().Select<ID3Tag, string>(x => x.Genre).Distinct();
                }
                Point pt = headerClicked.PointToScreen(new Point(0, 0));
                colSelector = new UserControls.ColumnSelector(distvals);
                colSelector.Top = pt.Y - colSelector.Height;
                colSelector.Left = pt.X;
                colSelector.ShowDialog();
                if(colSelector.SelectedValue.Length>0)
                { 
                    foreach (object item in dgImport.Items)
                    {
                        tag = item as ID3Tag;
                        if (headerClicked.Column == dg_tracknr)
                        {
                            tag.TrackNumber = Int32.Parse(colSelector.SelectedValue);
                        }
                        else if (headerClicked.Column == dg_title)
                        {
                            tag.Title = colSelector.SelectedValue;
                        }
                        else if (headerClicked.Column == dg_artist)
                        {
                            tag.Artist = colSelector.SelectedValue;
                        }
                        else if (headerClicked.Column == dg_album)
                        {
                            tag.Album = colSelector.SelectedValue;
                        }
                        else if (headerClicked.Column == dg_year)
                        {
                            tag.Year = Int32.Parse(colSelector.SelectedValue);
                        }
                        else if (headerClicked.Column == dg_genre)
                        {
                            tag.Genre = colSelector.SelectedValue;
                        }
                    }
                    try
                    {
                        dgImport.Items.Refresh();
                    }
                    catch
                    { 
                    }
                }
                 
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            UserControls.AboutDialog aboutDlg;
            aboutDlg = new UserControls.AboutDialog(System.Reflection.Assembly.GetExecutingAssembly());
            aboutDlg.Show();
        }

        private void visualssettings_Click(object sender, RoutedEventArgs e)
        {
            VisualsSettings vsettings = new VisualsSettings(_userSettings.Visuals);
            vsettings.ShowDialog();
        }

        private void importSettings_Click(object sender, RoutedEventArgs e)
        {
            ImportSettingsDialog isDlg;
            isDlg = new ImportSettingsDialog();
            isDlg.DataContext = _userSettings.ImportSettings;
            isDlg.ShowDialog();
            _userSettings.ImportSettings = (MusicBucketImportSettings)isDlg.DataContext;
        }

        #endregion

        #region player

        private void lvFiles_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.GetPosition(lvFiles).X < lvFiles.ActualWidth - 20 && e.GetPosition(lvFiles).Y < lvFiles.ActualHeight - 20)
            {
                _dragdropallowed = true;
            }
            else
            {
                _dragdropallowed = false;
            }
        }
        /// <summary>
        /// initializes the drag drop process from the middle list to the right (player) list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvFiles_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(lvFiles.SelectedItems.Count>0)
            {
                if (e.LeftButton == MouseButtonState.Pressed && _dragdropallowed)
                {
                    System.Windows.DataObject dobj;
                    dobj = new System.Windows.DataObject(_DRAGCONTENTTYPE, lvFiles.SelectedItems);
                    System.Windows.DragDrop.DoDragDrop(lvFiles, dobj, System.Windows.DragDropEffects.All);
                }
            }
            
        }

        private void lvPlayer_Drop(object sender, System.Windows.DragEventArgs e)
        {

                IList insertList;
                object dd = e.Data.GetData(typeof(object));
                insertList = (IList)e.Data.GetData(_DRAGCONTENTTYPE);
                for (int k = insertList.Count - 1; k > -1;k-- )
                {
                        PlayerQueue.Insert(0, (Mp3File)insertList[k]);
                }

        }

        private void lvPlayer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.GetPosition(lvPlayer).X < lvFiles.ActualWidth - 20 && e.GetPosition(lvPlayer).Y < lvFiles.ActualHeight - 20)
            {
                _dragdropplayerallowed = true;
            }
            else
            {
                _dragdropplayerallowed = false;
            }
        }

        private void lvPlayer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            List<Mp3File> toRemove;
            if (_dragdropplayerallowed && lvPlayer.SelectedItems.Count > 0 && e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.GetPosition(lvPlayer).X > lvFiles.ActualWidth ||
                    e.GetPosition(lvPlayer).Y > lvFiles.ActualHeight ||
                    e.GetPosition(lvPlayer).X < 0 ||
                    e.GetPosition(lvPlayer).Y < 0)
                {
                    toRemove = new List<Mp3File>();
                    foreach (object obj in lvPlayer.SelectedItems)
                    {
                        toRemove.Add((Mp3File)obj);
                    }
                    foreach (Mp3File mf in toRemove)
                    {
                        PlayerQueue.Remove(mf);
                    }
                    _dragdropplayerallowed = false;
                }

            }
        }

        private void buttonPlayerPlay_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer player=(MediaPlayer)Resources["MPlayer"];

            if (PlayerQueue.Count > 0 && !_playerIsPlaying && !_playerIsPaused)
            {
                string path = PlayerQueue.Last().FullPath;
                if(path.StartsWith("["))
                {
                }
                else
                {
                    player.Open(new Uri(path));
                    _playerIsPlaying = true;
                    _playerIsPaused = false;
                }
            }
            else if (_playerIsPaused)
            {
                _playerIsPaused = false;
                _playerIsPlaying = true;
                player.Play();
            }
        }

        private void buttonPlayerPause_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer player = (MediaPlayer)Resources["MPlayer"];
            if (_playerIsPlaying)
            {
                _playerIsPlaying = false;
                _playerIsPaused = true;
                player.Pause();
            }
        }

        private void mplayer_MediaOpened(object sender, EventArgs e)
        {
            MediaPlayer player = (MediaPlayer)Resources["MPlayer"];
            player.Play();
        }

        private void buttonPlayerStop_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer player = (MediaPlayer)Resources["MPlayer"];
            if (_playerIsPlaying || _playerIsPaused)
            {
                player.Stop();
                _playerIsPlaying = false;
                _playerIsPaused = false;
                player.Close();
            }
        }

        private void mplayer_MediaEnded(object sender, EventArgs e)
        {
            MediaPlayer player = (MediaPlayer)Resources["MPlayer"];
            player.Close();
            if (PlayerQueue.Count > 0)
            {
                PlayerQueue.RemoveAt(PlayerQueue.Count - 1);
                string path = PlayerQueue.Last().FullPath;
                if (path.StartsWith("["))
                {
                }
                else
                {
                    player.Open(new Uri(path));
                    _playerIsPlaying = true;
                    _playerIsPaused = false;
                }
            }
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            MediaPlayer player = (MediaPlayer)Resources["MPlayer"];
            if (_playerIsPlaying || _playerIsPaused)
            {
                try
                {
                    sliderPosition.Value = (player.Position.TotalSeconds / player.NaturalDuration.TimeSpan.TotalSeconds) * 1000.0;
                }
                catch
                { 
                }
            }
            else
            {
                sliderPosition.Value = 0;
            }
        }

        #endregion


    }

    class OldWindow : System.Windows.Forms.IWin32Window
    {
        private readonly System.IntPtr _handle;
        public OldWindow(System.IntPtr handle)
        {
            _handle = handle;
        }

        #region IWin32Window Members
        System.IntPtr System.Windows.Forms.IWin32Window.Handle
        {
            get { return _handle; }
        }
        #endregion
    }
}
