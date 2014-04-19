using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDRipperLib
{
    /// <summary>
    /// class containing all IOCTL control codes
    /// for CD-Rom as static Properties
    /// </summary>
    public class CDROMControlCodes
    {
        private static uint _METHOD_BUFFERED = 0,
            //_METHOD_IN_DIRECT = 1,
            _METHOD_OUT_DIRECT = 2,
            //_METHOD_NEITHER = 3,
            _FILE_DEVICE_CD_ROM = 2,
            _FILE_DEVICE_MASS_STORAGE = 45,
            _FILE_READ_ACCESS = 1,
            _FILE_ANY_ACCESS = 0,
            _FILE_WRITE_ACCESS = 2;


        public static uint IOCTL_CDROM_READ_TOC
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 0, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_SEEK_AUDIO_MSF
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 1, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_STOP_AUDIO
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 2, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_PAUSE_AUDIO
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 3, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_RESUME_AUDIO
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 4, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_GET_VOLUME
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 5, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_PLAY_AUDIO_MSF
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 6, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_SET_VOLUME
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 10, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_READ_Q_CHANNEL
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 11, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint OBSOLETE_IOCTL_CDROM_GET_CONTROL
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 13, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_GET_LAST_SESSION
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 14, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_RAW_READ
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 15, _METHOD_OUT_DIRECT, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_DISK_TYPE
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 16, _METHOD_BUFFERED, _FILE_ANY_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_GET_DRIVE_GEOMETRY
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 19, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_GET_DRIVE_GEOMETRY_EX
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 20, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_READ_TOC_EX
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 21, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_GET_CONFIGURATION
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 12, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_EXCLUSIVE_ACCESS
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 23, _METHOD_BUFFERED, _FILE_READ_ACCESS | _FILE_WRITE_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_SET_SPEED
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 24, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_GET_INQUIRY_DATA
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 25, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_ENABLE_STREAMING
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 26, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_SEND_OPC_INFORMATION
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 27, _METHOD_BUFFERED, _FILE_READ_ACCESS | _FILE_WRITE_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_GET_PERFORMANCE
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 28, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_CHECK_VERIFY
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 512, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_MEDIA_REMOVAL
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 513, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_EJECT_MEDIA
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 514, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_LOAD_MEDIA
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 515, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_RESERVE
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 516, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_RELEASE
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 517, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_CDROM_FIND_NEW_DEVICES
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_CD_ROM, 518, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        public static uint IOCTL_STORAGE_MEDIA_REMOVAL
        {
            get
            {
                return CTL_CODE(_FILE_DEVICE_MASS_STORAGE, 513, _METHOD_BUFFERED, _FILE_READ_ACCESS);
            }
        }

        private static uint CTL_CODE(uint devicetype, uint function, uint method, uint access)
        {
            return (devicetype << 16) | (access << 14) | (function << 2) | method;
        }
    }
}
