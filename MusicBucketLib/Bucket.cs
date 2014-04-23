using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using System.ComponentModel;
using MP3Tagger.Classes;
using PTCAccess;
namespace MusicBucketLib
{
    /// <summary>
    /// Defines as bucket, a storage location for mp3s.
    /// </summary>
    [Serializable]
    public class Bucket : ISerializable,INotifyPropertyChanged
    {
        private string _bucketPath="";
        private string _title = "";
        private Nullable<int> _nfiles;
        private bool _isAttached;
        private bool _isDirty;
        private bool _includeSubfolders;

        public delegate void Mp3FileReadHandler(Mp3File file);
        public event Mp3FileReadHandler Mp3FileRead;
        /// <summary>
        /// Initalizes a new instance of <see cref="Bucket"/>.
        public Bucket()
        { 
            _nfiles=null;
            _isAttached = false;
            _isDirty = false;
        }

        /// <summary>
        /// Deserialization constructor, bucket is set dirty automatically since all data is read from serialization
        /// object instead of detecting phenomenologically.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public Bucket(SerializationInfo info, StreamingContext context)
        {
            _bucketPath = (string)info.GetValue("Path", typeof(string));
            _nfiles = (Nullable<int>)info.GetValue("NFiles", typeof(Nullable<int>));
            _title = (string)info.GetValue("Title", typeof(string));
            _includeSubfolders = (bool)info.GetValue("IncludeSubFolders", typeof(bool));
            _isDirty = true;
        }

        /// <summary>
        /// The path to the directory defining the bucket. Also sets <see cref="IsAttached"/> de^pending on if the path actually exists
        /// </summary>
        public string Path
        {
            get
            {
                return _bucketPath;
            }
            set
            {
                _isDirty = true;
                _bucketPath = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Path"));
                }

            }
        }

        /// <summary>
        /// Gets of sets the number of files in the bucket. It can be set artificially using the setter. In this case the bucket
        /// is set dirty meaning that the number entered does not certainly reflect the true number of files. 
        /// If the value has never been set it is automatically computed. If it is set dirty the the "dirty" value is taken.
        /// </summary>
        public Nullable<int> NumberFiles
        {
            get
            {
                DirectoryInfo dirInfo;
                if(_nfiles==null) // count files if not already set
                {
                    if (PathIsPortableDevice(_bucketPath))
                    {
                        PTCFolder fld;
                        this.IsAttached = PTCWrapper.Exists(_bucketPath,out fld);
                        if (this.IsAttached)
                        {
                            _nfiles = PTCWrapper.GetMp3FileNames(fld).Count;
                            try
                            {
                                PropertyChanged(this, new PropertyChangedEventArgs("NumberFiles"));
                            }
                            catch { }
                        }
                    }
                    else
                    { 
                        this.IsAttached = Directory.Exists(_bucketPath);
                        if (this.IsAttached)
                        {
                            dirInfo = new DirectoryInfo(_bucketPath);
                            _nfiles = dirInfo.GetFiles("*.mp3",_includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Length;
                            try
                            {
                                PropertyChanged(this, new PropertyChangedEventArgs("NumberFiles"));
                            }
                            catch { }
                        
                        }
                        else
                        {
                            _nfiles = null;
                        }
                    }
                }
                    return _nfiles;
            }
            set
            {
                _nfiles=value;
                _isDirty=true;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("NumberFiles"));
                }
            }
        }

        /// <summary>
        /// Returns the last value of the "isAttached" variable set. Does not have to be in sync with the actual attachedness of the bucket.
        /// </summary>
        public bool IsAttached
        {
            get
            {
                return _isAttached;
            }
            set
            {
                _isAttached = value;
                _isDirty = true;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsAttached"));
                }
            }
        }

        public bool IncludeSubFolders
        {
            get
            {
                return _includeSubfolders;
            }
            set
            {
                _includeSubfolders = value;
                _isDirty = true;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IncludeSubFolders"));
                }
            }
        }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Title"));
                }
            }
        }


        /// <summary>
        /// Sets the parameters <see cref="NumberFiles"/> and <see cref="IsAttached"/> according to the effective values. Only does the update
        /// if the internal dirty flags is set or the optional force parameter is set true. This happens when one sets any of the properties.
        /// </summary>
        /// <param name="force">does the update independent of the state if the dirty flag if set true.</param>
        public void Update(bool force = false)
        {
            if (force || _isDirty)
            {
                _nfiles = null;
                int? dummy = this.NumberFiles;
                _isDirty = false;
            }
        }

        public ObservableCollection<Mp3File> GetMp3Files(BackgroundWorker worker=null,DoWorkEventArgs e=null)
        {
            DirectoryInfo dirInfo;
            FileInfo[] files;
            Stream fstream;
            ID3v1 tagv1;
            ID3v23 tagv23;
            Mp3File mp3 = new Mp3File();
            ObservableCollection<Mp3File> res = new ObservableCollection<Mp3File>();
            if (worker != null)
            {
                worker.ReportProgress(2, Properties.Resources.ReadingBucketProgress1);
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return res;
                }
            }
            this.Update(true);
            if (worker != null)
            {
                worker.ReportProgress(2, Properties.Resources.ReadingBucketProgress2);
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return res;
                }
            }
            if(IsAttached)
            {
                if (PathIsPortableDevice(_bucketPath))
                {
                    PTCFolder fld;
                    List<PTCFile> mp3s;
                    PTCWrapper.Exists(_bucketPath, out fld);
                    if (worker != null)
                    {
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            return res;
                        }
                    }
                    mp3s = PTCWrapper.GetMp3FileNames(fld);
                    if (worker != null)
                    {
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            return res;
                        }
                    }
                    foreach (PTCFile file in mp3s)
                    {
                        mp3 = new Mp3File();
                        mp3.Tags = new List<MP3Tagger.Classes.ID3Tag>();
                        try
                        {
                            fstream = PTCWrapper.GetMp3Stream(file);
                            try
                            {
                                tagv1 = new ID3v1();
                                tagv23 = new ID3v23();
                                if (tagv1.Read(fstream))
                                {
                                    mp3.Tags.Add(tagv1);

                                }
                                if (tagv23.Read(fstream))
                                {
                                    mp3.Tags.Add(tagv23);
                                }

                            }
                            catch (Exception exc)
                            {
                                string test = exc.Message;
                            }
                            finally
                            {
                                fstream.Close();
                            }
                        }
                        catch
                        {

                        }
                        mp3.FullPath =  _bucketPath + file.Name;
                        mp3.Filename = file.Name;
                        res.Add(mp3);
                        if (Mp3FileRead != null)
                        {
                            Mp3FileRead(mp3);
                        }
                        if (worker != null)
                        {
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return res;
                            }
                        }
                    }
                }
                else
                {
                    dirInfo = new DirectoryInfo(_bucketPath);
                    files = dirInfo.GetFiles("*.mp3", _includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                    foreach (FileInfo finfo in files)
                    {
                        mp3 = new Mp3File();
                        mp3.Tags = new List<MP3Tagger.Classes.ID3Tag>(); // new MP3Tagger.Classes.ID3v1();
                        try
                        {
                            fstream = new FileStream(finfo.FullName, FileMode.Open, FileAccess.Read);
                            try
                            {
                                tagv1 = new ID3v1();
                                tagv23 = new ID3v23();
                                if (tagv1.Read(fstream))
                                {
                                    mp3.Tags.Add(tagv1);

                                }
                                if (tagv23.Read(fstream))
                                {
                                    mp3.Tags.Add(tagv23);
                                }
                                fstream.Close();

                            }
                            catch
                            {

                            }
                            finally
                            {
                                fstream.Close();
                            }
                        }
                        catch
                        {

                        }

                        mp3.FullPath = finfo.FullName;
                        mp3.Filename = finfo.FullName.Split('\\').Last<string>();
                        res.Add(mp3);
                        if (Mp3FileRead != null)
                        {
                            Mp3FileRead(mp3);
                        }
                        if (worker != null)
                        {
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return res;
                            }
                        }
                    }
                }
            }
            return res;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Path", this.Path);
            info.AddValue("NFiles", this.NumberFiles);
            info.AddValue("IncludeSubFolders", this.IncludeSubFolders);
            info.AddValue("Title", this.Title);
        }


        private bool PathIsPortableDevice(string path)
        {
            return path.StartsWith("[");
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
