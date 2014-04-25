using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3Tagger.Classes
{
    public class CommentTagFrameV22 : TagFrameV22
    {
        byte[] _language;
        string _shortdescr,_content;
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
                return _content;
            }
            set
            {
                _content = value;
            }
        }

        public CommentTagFrameV22()
        {
            _language = Encoding.ASCII.GetBytes("eng");
            _enc = new byte[]{0};
            _encObj = Encoding.GetEncoding("ISO-8859-1");

        }

        public CommentTagFrameV22(TagFrameV22 tf)
        {
            Encoding enc;
            _enc = new byte[3];
            byte cur1, cur2;
            List<byte> bbfr;
            int cnt = 0;
            bbfr = new List<byte>();
            this.Content = tf.Content;
            this.FrameHeader = tf.FrameHeader;
            _language = tf.Content.Skip(1).Take(3).ToArray();
            _enc[0] = tf.Content[0];
            if (_enc[0] == 1)
            {
                _enc[1] = tf.Content.Skip(4).Take(1).First();
                _enc[2] = tf.Content.Skip(5).Take(1).First();
            }
            enc = ID3v22.GetEncoding(_enc);
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
                _shortdescr = enc.GetString(bbfr.ToArray(),0,bbfr.Count);
                cnt += 2;
                cur1 = tf.Content[4 + cnt];
                cur2 = tf.Content[5 + cnt];
                bbfr.Clear();
                while (cur1 != 0 && cur2 != 0 && cnt-4<tf.FrameSize)
                {
                    bbfr.Add(cur1);
                    bbfr.Add(cur2);
                    cnt += 2;
                    cur1 = tf.Content[4 + cnt];
                    cur2 = tf.Content[5 + cnt];
                }
                _content = enc.GetString(bbfr.ToArray(), 0, bbfr.Count);
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
                _shortdescr = enc.GetString(bbfr.ToArray(), 0, bbfr.Count);
                cnt++;
                cur1 = tf.Content[4 + cnt];
                bbfr.Clear();
                while (cur1 != 0 && cnt - 4 < tf.FrameSize)
                {
                    bbfr.Add(cur1);
                    cnt += 1;
                    cur1 = tf.Content[4 + cnt];
                }
                _content = enc.GetString(bbfr.ToArray(), 0, bbfr.Count);
            }
        }

        public override void Save(System.IO.Stream outstream)
        {
            byte[] bbfr,bbfr1,bbfr2;
            int size = 4;
            bbfr1=_encObj.GetBytes(_shortdescr);

            size += bbfr1.Length;
            bbfr2 = _encObj.GetBytes(_content);

            size += bbfr2.Length;

            bbfr = System.Text.Encoding.ASCII.GetBytes(FrameHeader);
            outstream.Write(bbfr, 0, 3);
            outstream.Write(BitConverter.GetBytes(size).Reverse().ToArray(), 0, 3);
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
