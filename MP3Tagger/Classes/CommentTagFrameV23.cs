using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3Tagger.Classes
{
    class CommentTagFrameV23 : TagFrameV23
    {
        byte[] _language;
        string _shortdescr,_mainComment;
        byte[] _enc;
        Encoding _encObj;

        public byte[] Language
        {
            get
            {
                return _language;
            }
            set
            {
                _language = value;
            }
        }

        public string ShortDescription
        {
            get
            {
                return _shortdescr;
            }
            set
            {
                _shortdescr = value;
            }
        }

        public string MainComment
        {
            get
            {
                return _mainComment;
            }
            set
            {
                _mainComment = value;
            }
        }

        public CommentTagFrameV23()
        {
            _language = Encoding.ASCII.GetBytes("eng");
            _enc = new byte[]{0};
            _encObj = Encoding.GetEncoding("ISO-8859-1");
            _mainComment = "";
            _shortdescr = "";
            this.FrameHeader = ID3v23.COMMENTS;
            this.Content = _enc.Concat(_language).Concat(new byte[] { 0 }).ToArray();
            this.Flags = new byte[] { 0, 0 };

        }

        public CommentTagFrameV23(TagFrameV23 tf)
        {
            _enc = new byte[3];
            byte cur1, cur2;
            List<byte> bbfr;
            int cnt = 0;
            bbfr = new List<byte>();
            this.Content = new byte[0]; // delete content on purpose (saving is implemented specifically for the content tag)
            this.FrameHeader = tf.FrameHeader;
            this.Flags = tf.Flags;
            _language = tf.Content.Skip(1).Take(3).ToArray();
            _enc[0] = tf.Content[0];
            if (_enc[0] == 1)
            {
                _enc[1] = tf.Content.Skip(4).Take(1).First();
                _enc[2] = tf.Content.Skip(5).Take(1).First();
            }
            _encObj = ID3v23.GetEncoding(_enc);
            //read short description
            if (_enc[0] == 1) 
            {
                cur1 = tf.Content[4 + cnt];
                cur2 = tf.Content[5 + cnt];
                while(cur1!= 0 && cur2!=0)
                {
                    bbfr.Add(cur1);
                    bbfr.Add(cur2);
                    cnt += 2;
                    cur1 = tf.Content[4 + cnt];
                    cur2 = tf.Content[5 + cnt];
                }
                _shortdescr = _encObj.GetString(bbfr.ToArray(),0,bbfr.Count);
                cnt += 2;
                cur1 = tf.Content[4 + cnt];
                cur2 = tf.Content[5 + cnt];
                bbfr.Clear();
                while (cur1 != 0 && cur2 != 0 && cnt + 6 < tf.FrameSize)
                {
                    bbfr.Add(cur1);
                    bbfr.Add(cur2);
                    cnt += 2;
                    cur1 = tf.Content[4 + cnt];
                    cur2 = tf.Content[5 + cnt];
                } 
                _mainComment = _encObj.GetString(bbfr.ToArray(), 0, bbfr.Count);
            }
            else
            {
                cur1 = tf.Content[4 + cnt];
                while (cur1 != 0)
                {
                    bbfr.Add(cur1);
                    cnt += 1;
                    cur1 = tf.Content[4 + cnt];
                }
                _shortdescr = _encObj.GetString(bbfr.ToArray(), 0, bbfr.Count);
                cnt++;
                if (cnt + 5 < tf.FrameSize)
                {
                    cur1 = tf.Content[4 + cnt];
                    bbfr.Clear();
                    while (cur1 != 0 && cnt + 5 < tf.FrameSize)
                    {
                        bbfr.Add(cur1);
                        cnt += 1;
                        cur1 = tf.Content[4 + cnt];
                    }
                    _mainComment = _encObj.GetString(bbfr.ToArray(), 0, bbfr.Count);
                }
                else
                {
                    _mainComment = "";
                }
            }
        }
        public override uint FrameSize
        {
            get 
            {
                return 5 + (uint)_encObj.GetByteCount(_shortdescr) + (uint)_encObj.GetByteCount(_mainComment);
            }
        }

        public override void Save(System.IO.Stream outstream)
        {
            byte[] bbfr,bbfr1,bbfr2;
            int size = 5;

            bbfr1 = _encObj.GetBytes(_shortdescr);

            size += bbfr1.Length;
            bbfr2 = _encObj.GetBytes(_mainComment);

            size += bbfr2.Length;

            bbfr = System.Text.Encoding.ASCII.GetBytes(FrameHeader);
            outstream.Write(bbfr, 0, 4);
            outstream.Write(BitConverter.GetBytes(size).Reverse().ToArray(), 0, 4);
            outstream.Write((this as TagFrameV23).Flags, 0, 2);
            outstream.Write(_enc, 0, 1);
            outstream.Write(_language, 0, 3);
            outstream.Write(bbfr1, 0, bbfr1.Length);
            outstream.WriteByte(0);
            if (_enc[0] == 1)
            {
                outstream.WriteByte(0);
            }
            outstream.Write(bbfr2, 0, bbfr2.Length);
        }
    }
}
