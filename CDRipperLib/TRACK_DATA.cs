using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CDRipperLib
{
    [StructLayout(LayoutKind.Sequential)]
    public class TRACK_DATA
    {
        private byte _Reserved;
        private byte _ControlAdr;
        private byte _TrackNumber;
        private byte _Reserved1;
        private byte _Address1;
        private byte _Address2;
        private byte _Address3;
        private byte _Address4;

        public TRACK_DATA(byte[] inArray,int index)
        {
            this._Reserved1 = inArray[index*8 + 0];
            this._ControlAdr = inArray[index * 8 + 1];
            this._TrackNumber = inArray[index * 8 + 2];
            this._Reserved1 = inArray[index * 8 + 3];
            this._Address1 = inArray[index * 8 + 4];
            this._Address2 = inArray[index * 8 + 5];
            this._Address3 = inArray[index * 8 + 6];
            this._Address4 = inArray[index * 8 + 7];
        }

        public int TrackNumber
        {
            get
            {
                return (int)_TrackNumber;
            }
        }

        public byte Adr
        {
            get
            {
                return (byte)(_ControlAdr >> 4);
            }
        }

        public byte Control
        {
            get
            {
                return (byte)(15 & _ControlAdr);
            }
        }

        public int StartSector
        {
            get
            {
                return (_Address2 * 60 * 75 + _Address3 * 75 + _Address4 - 150);
            }
        }
    }

}
