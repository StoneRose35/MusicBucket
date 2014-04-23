using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3Tagger.Classes
{
    /// <summary>
    /// unicode encoding which removes 
    /// the BOM from string after conversion from byte array since this is troublesome when converting the string to other formats
    /// re-adds it when converting a string to a byte array
    /// </summary>
    public class UnicodeBOM : UnicodeEncoding
    {

        public UnicodeBOM() : base(false,true) { } // default constructor encodes in little-endian

        public UnicodeBOM(bool endiannness) : base(endiannness, true) { }


        public override byte[] GetBytes(string s)
        {
            List<byte> blist;
            //remove existing bom occurences
            if (s[0] == (char)65534 || s[0] == (char)65279)
            {
                s = s.Substring(1);
            }
            blist = new List<byte>();
            blist.AddRange(base.GetPreamble());
            blist.AddRange(base.GetBytes(s));
            return blist.ToArray();
        }

        public override string GetString(byte[] bytes)
        {
            if ((bytes[0] == 255 && bytes[1] == 254)) // little-endianness
            {
                return Encoding.Unicode.GetString(bytes.Skip(2).ToArray());
            }
            else if ((bytes[0] == 254 && bytes[1] == 255)) // big-endianness
            {
                return Encoding.BigEndianUnicode.GetString(bytes.Skip(2).ToArray());
            }
            else
            {
                return Encoding.Unicode.GetString(bytes);
            }

        }
    }
}
