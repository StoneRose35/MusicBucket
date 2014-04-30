using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CDRipperLib
{
    public class CDRipper
    {

        private const int NSECTORS = 10;
        public const int SECTORSIZE=2352;
        private IntPtr _deviceHandler;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateFile(string filename, FileAccess access, FileShare sharing,
              IntPtr SecurityAttributes, FileMode mode, FileOptions options, IntPtr template
        );
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr device, uint ctlcode,
            IntPtr inbuffer, int inbuffersize,
            IntPtr outbuffer, int outbufferSize,
            IntPtr bytesreturned, IntPtr overlapped
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int DeviceIoControl(IntPtr device, uint ctlcode,
            RAW_READ_INFO inbuffer, int inbuffersize,
            byte[] data, int outbufferSize,
            IntPtr bytesreturned, IntPtr overlapped
        );

        [DllImport("kernel32.dll")]
        private static extern void CloseHandle(IntPtr hdl);

        public event NotifyProgressHandler NotifyProgress;
        public delegate void NotifyProgressHandler(int tracknumber,int progress);
        public CDRipper(string directoryName)
        {
            string cfilename;
            cfilename = "\\\\.\\" + directoryName + ":";
            _deviceHandler = CreateFile(cfilename, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileOptions.None, IntPtr.Zero);

        }

        public CDROM_TOC READ_TOC()
        {
            CDROM_TOC toc=new CDROM_TOC();
            int outsize = Marshal.SizeOf(typeof(CDROM_TOC));
            IntPtr outbfr = Marshal.AllocHGlobal(outsize);
            bool result;
            try
            {
                result = DeviceIoControl(_deviceHandler, CDROMControlCodes.IOCTL_CDROM_READ_TOC, IntPtr.Zero, 0, outbfr, outsize, IntPtr.Zero, IntPtr.Zero);
                Marshal.PtrToStructure(outbfr, toc);
                Marshal.FreeHGlobal(outbfr);
                if (toc.GetDiscID() == "0") // no cd inserted
                {
                    return null;
                }
                else
                {
                    return toc;
                }
            }
            catch
            {
                return null;
            }
        }

        public void ReadTrack(CDROM_TOC toc,int tracknumber,WaveWriter wvwriter)
        {
            IntPtr outpt;
            RAW_READ_INFO rri;
            byte[] data;
            int outsize;
            int bytesread;
            int totalsectors, sectorscnt=0;
            totalsectors = toc.GetTrackData(tracknumber + 1).StartSector - 1 - toc.GetTrackData(tracknumber).StartSector;
            while (sectorscnt < totalsectors)
            {
                if (totalsectors - sectorscnt < NSECTORS)
                {
                    rri = new RAW_READ_INFO(toc.GetTrackData(tracknumber).StartSector + sectorscnt, totalsectors - sectorscnt, TRACK_MODE_TYPE.CDDA);
                }
                else
                {
                    rri = new RAW_READ_INFO(toc.GetTrackData(tracknumber).StartSector + sectorscnt, NSECTORS, TRACK_MODE_TYPE.CDDA);
                }
                outpt = Marshal.AllocHGlobal(rri.BytesToRead);
                outsize = rri.BytesToRead;
                data = new byte[outsize];
                bytesread = DeviceIoControl(_deviceHandler, CDROMControlCodes.IOCTL_CDROM_RAW_READ, rri, Marshal.SizeOf(rri), data, outsize, IntPtr.Zero, IntPtr.Zero);
                sectorscnt = sectorscnt + NSECTORS;
                wvwriter.WriteChunk(data);
                if (NotifyProgress != null)
                {
                    NotifyProgress(tracknumber,(int)(((double)sectorscnt / (double)totalsectors) * 100));
                }
            }
        }

        public bool LockDrive()
        {
            IntPtr inptr;
            byte indata=1;
            inptr=(IntPtr)GCHandle.Alloc(indata);

            return DeviceIoControl(_deviceHandler, CDROMControlCodes.IOCTL_STORAGE_MEDIA_REMOVAL, inptr, 1, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
        }

        public bool UnLockDrive()
        {
            IntPtr inptr;
            byte indata = 0;
            inptr = (IntPtr)GCHandle.Alloc(indata);

            return DeviceIoControl(_deviceHandler, CDROMControlCodes.IOCTL_STORAGE_MEDIA_REMOVAL, inptr, 1, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
        }


        ~CDRipper()
        {
            CloseHandle(_deviceHandler);
        }
    }

}
