using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PTCAccess
{
    /// <summary>
    /// Class describing a multimedia device folder, only holds properties
    /// </summary>
    public class PTCFolder
    {
        private string _name,_uid,_id,_deviceName,_deviceID;
        private bool _isRootFolder;
        private PTCFolder _parent;
        /// <summary>
        /// The clear-text name of the Folder (this should be displayed in the UI)
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// A persistent unique ID, use thhis to retrieve properties or content of the folder
        /// </summary>
        public string Uid
        {
            get
            {
                return _uid;
            }
            set
            {
                _uid = value;
            }
        }

        /// <summary>
        /// a non-persistent ID, valid only within the scope of a session
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// If true then the folder is a virtual drive, if not it is a normal folder
        /// </summary>
        public bool IsRootFolder
        {
            get
            {
                return _isRootFolder;
            }
            set
            {
                _isRootFolder = value;
            }
        }

        public string DeviceName
        {
            get
            {
                return _deviceName;
            }
            set
            {
                _deviceName = value;
            }
        }

        public string DeviceID
        {
            get
            {
                return _deviceID;
            }
            set
            {
                _deviceID = value;
            }
        }

        public PTCFolder Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }
        /// <summary>
        /// Return a string representation of the object.
        /// </summary>
        /// <returns>The name of the folder</returns>
        public override string ToString()
        {
            return _name;
        }

        public override bool Equals(object obj)
        {
            if (obj is PTCFolder)
            {
                return ((PTCFolder)obj).Name == this.Name;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
