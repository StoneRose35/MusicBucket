using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using MP3Tagger.Exceptions;

namespace MP3Tagger.Classes
{
    public class TagFrameV22
    {
        private uint _tagsize;
        private string _tagheader;
        private byte[] _content;

        public uint FrameSize
        {
            get
            {
                _tagsize = (uint)_content.Length;
                return _tagsize;
            }
        }

        public string FrameHeader
        {
            get { return _tagheader; }
            set
            {
                if (value.Length == 3)
                {
                    _tagheader = value;
                }
                else
                {
                    throw new TagLengthWrongException();
                }
            }
        }

        public virtual void Save(Stream outstream)
        {
            byte[] bbfr;
            byte[] outbyte = new byte[_content.Length + 6];
            bbfr = Encoding.ASCII.GetBytes(FrameHeader);
            outstream.Write(bbfr, 0, 3);
            bbfr = BitConverter.GetBytes(FrameSize);
            bbfr = bbfr.Reverse().ToArray();
            outstream.Write(bbfr, 0, 3);
            outstream.Write(_content, 0, _content.Length);
        }

        public bool Read(Stream instream)
        {
            bool res = false;
            byte[] bbfr;
            bbfr = new byte[3];
            instream.Read(bbfr, 0, 3);
            _tagheader = Encoding.ASCII.GetString(bbfr);
            instream.Read(bbfr, 0, 3);
            _tagsize = (uint)(bbfr[0] << 16 | bbfr[1] << 8 | bbfr[2]);
            _content = new byte[_tagsize];
            instream.Read(_content, 0, (int)_tagsize);
            return res;
        }

        public byte[] Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public string ContentAsString
        {
            get
            {
                if (FrameHeader.StartsWith("T"))
                {
                    Encoding enc;
                    enc = ID3v23.GetEncoding(Content);
                    return enc.GetString(Content.Skip(1).ToArray());
                }
                else
                {
                    string outstring = "";
                    int flength = Content.Length > 256 ? 256 : Content.Length;
                    for (int k = 0; k < flength; k++)
                    {
                        if (Content[k] > 31)
                        {
                            outstring += (char)Content[k];
                        }
                        else
                        {
                            outstring += Encoding.Unicode.GetString(new byte[] { 59, 38 });//{ 38, 59 }); // the black smiley in lucida console
                        }
                    }
                    return outstring;
                }
            }
        }
    }
}
