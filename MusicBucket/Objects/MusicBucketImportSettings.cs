using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MusicBucket.Objects
{
    [Serializable]
    public class MusicBucketImportSettings: ISerializable,INotifyPropertyChanged
    {
        private bool _writeID3v1, _writeID3v22, _writeID3v23;
        private int _bitrate;

        public MusicBucketImportSettings()
        {
            WriteID3v1 = false;
            WriteID3v22 = false;
            WriteID3v23 = true;
            BitRate = 128;
        }

        public MusicBucketImportSettings(SerializationInfo info, StreamingContext context)
        {
            _writeID3v1 = (bool)info.GetValue("ID3v1", typeof(bool));
            _writeID3v22 = (bool)info.GetValue("ID3v22", typeof(bool));
            _writeID3v23 = (bool)info.GetValue("ID3v23", typeof(bool));
            _bitrate = (int)info.GetValue("BitRate", typeof(int));
        }

        public bool WriteID3v1
        {
            get { return _writeID3v1; }
            set 
            { 
                _writeID3v1 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("WriteID3v1"));
                }
            }
        }

        public bool WriteID3v2
        {
            get { return _writeID3v22 || _writeID3v23; }
            set 
            {
                if (value)
                {
                    WriteID3v23 = true;
                    WriteID3v22 = false;
                }
                else
                {
                    WriteID3v22 = false;
                    WriteID3v23 = false;
                }
            }
        }

        public bool WriteID3v22
        {
            get { return _writeID3v22; }
            set 
            {
                _writeID3v22 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("WriteID3v22"));
                }
                if (_writeID3v23 && _writeID3v22) // only one tags type of v2 can be written at once
                {
                    _writeID3v23 = false;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("WriteID3v23"));
                    }
                }
            }
        }

        public bool WriteID3v23
        {
            get { return _writeID3v23; }
            set
            {
                _writeID3v23 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("WriteID3v23"));
                }
                if (_writeID3v23 && _writeID3v22) // only one tags type of v2 can be written at once
                {
                    _writeID3v22 = false;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("WriteID3v22"));
                    }
                }
            }
        }

        public int BitRate
        {
            get { return _bitrate; }
            set 
            {
                switch(value)
                {
                    case 32:
                    case 40:
                    case 48:
                    case 56:
                    case 64:
                    case 80:
                    case 96:
                    case 112:
                    case 128:
                    case 160:
                    case 192:
                    case 224:
                    case 256:
                    case 320:
                        _bitrate = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("BitRate"));
                        }
                        break;
                    default:
                        throw new MusicBucketException(Properties.Resources.ImportSettingsDialogInvalidBitrate);
                }
            }
        }

        /// <summary>
        /// Object Deep-Copy
        /// </summary>
        /// <returns></returns>
        public MusicBucketImportSettings Clone()
        {
            MusicBucketImportSettings res;
            res = new MusicBucketImportSettings();
            res.WriteID3v1 = this.WriteID3v1;
            res.WriteID3v22 = this.WriteID3v22;
            res.WriteID3v23 = this.WriteID3v23;
            res.BitRate = this.BitRate;
            return res;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ID3v1", this.WriteID3v1);
            info.AddValue("ID3v22", this.WriteID3v22);
            info.AddValue("ID3v23", this.WriteID3v23);
            info.AddValue("BitRate", this.BitRate);
        }
    }
}
