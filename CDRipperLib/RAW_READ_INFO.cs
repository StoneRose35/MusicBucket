using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace CDRipperLib
{
    public enum TRACK_MODE_TYPE
    {
        YellowMode2          = 0,
        XAForm2              = 1,
        CDDA                 = 2,
        RawWithC2AndSubCode  = 3,
        RawWithC2            = 4,
        RawWithSubCode       = 5
    }
    [StructLayout(LayoutKind.Sequential)]
    public class RAW_READ_INFO
    {
        private Int64 _diskoffset;
        private UInt32 _sectorcount;
        private TRACK_MODE_TYPE _trackModeType;

        public RAW_READ_INFO(int startSector, int numbersectors, TRACK_MODE_TYPE mt)
        {
            _diskoffset = ((Int64)startSector) * 2048;
            _sectorcount = Convert.ToUInt32(numbersectors);
            _trackModeType = mt;
        }

        public int BytesToRead
        {
            get
            {
                return (int)_sectorcount * CDRipper.SECTORSIZE;
            }
        }
    }
}
