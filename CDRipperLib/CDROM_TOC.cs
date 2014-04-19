using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CDRipperLib
{
    [StructLayout(LayoutKind.Sequential)]
    public class CDROM_TOC
    {
        private byte length1;
        private byte length2;
        private byte firstTrack;
        private byte lastTrack;
        [MarshalAs(UnmanagedType.ByValArray,SizeConst=800)]
        private byte[] TRACK_DATA=new byte[800];



        public int FirstTrack
        {
            get
            {
                return (int)firstTrack;
            }
        }

        public int LastTrack
        {
            get
            {
                return (int)lastTrack;
            }
        }

        public int Length
        {
            get
            {
                return (length1 << 8) | length2;
            }
        }

        public TRACK_DATA GetTrackData(int index)
        {
            TRACK_DATA trdata=new TRACK_DATA(TRACK_DATA,index);
            return trdata;
        }

        public string GetDiscID()
        {
            byte byte3 = lastTrack,byte0; 
            ushort totallength;
            int checksum=0;
            uint total;
            totallength=(ushort)((GetTrackData(lastTrack).StartSector - GetTrackData(0).StartSector)/75);
            byte[] tlbytes=BitConverter.GetBytes(totallength);
            for (int k = 0; k < lastTrack; k++)
            {
                checksum += GetSumOfDigits((GetTrackData(k).StartSector+150) / 75);
            }
            byte0 = (byte)(checksum % 255);
            total = (uint)(byte0 << 24 | tlbytes[0] << 16 | tlbytes[1] << 8 | byte3);
            return total.ToString("X");
        }

        private int GetSumOfDigits(int n)
        {
            int res = 0;
            while (n > 0)
            {
                res=res + (n%10);
                n = n / 10;
            }
            return res;
        }
    }

}
