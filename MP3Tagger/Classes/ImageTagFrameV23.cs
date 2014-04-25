using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
namespace MP3Tagger.Classes
{
    public class ImageTagFrameV23 : TagFrameV23
    {
        private string _mimetype="image/jpeg"; // jpeg as default
        private string _descr = "";
        private byte _pictype = 3; // front cover as default
        private byte[] _enc;
        private Image _img;
        public string MimeType
        {
            get
            {
                return _mimetype;
            }
            set
            {
                _mimetype = value;
            }
        }

        public byte[] Encoding
        {
            set { _enc = value; }
        }

        public string Description
        {
            get
            {
                return _descr;
            }
            set
            {
                _descr = value;
            }
        }

        public byte PictureType
        {
            get { return _pictype; }
            set { _pictype = value; }
        }

        public Image CoverImage
        {
            get { return _img; }
            set {
                MemoryStream memstream;
                _img = value;
                memstream = new MemoryStream();
                this.Save(memstream);
                base.Content=memstream.ToArray();
                memstream.Close();
            }
        }

        public override void Save(Stream outstream)
        {
            byte[] bbfr;
            MemoryStream memstream;
            System.Drawing.Imaging.ImageFormat imgfmt;
            uint framesize = 0; // string terminations, encoding and picutre type (4 or 5)
            System.Text.Encoding encod;
            List<byte> totalcontent = new List<byte>();
            encod = ID3v23.GetEncoding(_enc);
            if (_enc[0] == 0)
            {
                framesize = 4;
            }
            else
            {
                framesize = 5;
            }
            byte[] mime,desc;
            mime=System.Text.Encoding.ASCII.GetBytes(MimeType);
            desc=encod.GetBytes(Description);
            framesize += (uint)mime.Length;
            framesize += (uint)desc.Length;
            memstream = new MemoryStream(2000000);
            if(MimeType=="image/jpeg")
            {
                imgfmt=System.Drawing.Imaging.ImageFormat.Jpeg;
            }
            else if (MimeType=="image/png")
            {
                imgfmt=System.Drawing.Imaging.ImageFormat.Png;
            }
            else
            {
                imgfmt=System.Drawing.Imaging.ImageFormat.Png;
            }
            CoverImage.Save(memstream, imgfmt);
            framesize += (uint)memstream.Length;
            bbfr = System.Text.Encoding.ASCII.GetBytes(FrameHeader);
            outstream.Write(bbfr, 0, 4);
            outstream.Write(BitConverter.GetBytes(framesize).Reverse().ToArray(), 0, 4);
            outstream.Write(Flags, 0, 2);
            outstream.WriteByte(_enc[0]);
            outstream.Write(mime, 0, mime.Length);
            outstream.WriteByte(0);
            outstream.WriteByte(PictureType);
            outstream.Write(desc, 0, desc.Length);
            outstream.WriteByte(0);
            if(_enc[0]==1)
            {
                outstream.WriteByte(0);
            }
            outstream.Write(memstream.ToArray(), 0, (int)memstream.Length);
            memstream.Close();

        }

        public ImageTagFrameV23()
        {
            _enc = new byte[]{0};
            _mimetype = "image/jpeg";
            _pictype = 3;
        }

        public ImageTagFrameV23(TagFrameV23 tf)
        {
            Encoding enc;
            byte current,current2;
            int bytesread = 0;
            List<byte> mimetypebyte;
            List<byte> descriptionbyte;
            byte[] bbfr;
            this.Content = tf.Content;
            this.Flags = tf.Flags;
            this.FrameHeader = tf.FrameHeader;
            this.Encoding = tf.Content.Take(3).ToArray();

            bytesread++;
            current = tf.Content[bytesread];
            bytesread++;

            mimetypebyte = new List<byte>();
            while (current != 0)
            {
                mimetypebyte.Add(current);
                current = tf.Content[bytesread];
                bytesread++;
            }
            this.MimeType = System.Text.Encoding.ASCII.GetString(mimetypebyte.ToArray()); //mime type encoding is always ascii
            this.PictureType = tf.Content[bytesread];
            bytesread++;
            descriptionbyte = new List<byte>();
            if (_enc[0] == 0) // Latin1 encoding (singlebyte)
            {
                current = tf.Content[bytesread];
                bytesread++;
                while (current != 0)
                {
                    descriptionbyte.Add(current);
                    current = tf.Content[bytesread];
                    bytesread++;
                }
            }
            else // Unicode Encoding (2 bytes)
            {
                current = tf.Content[bytesread];
                _enc[1] = current;
                bytesread++;
                current2 = tf.Content[bytesread];
                _enc[2] = current2;
                bytesread++;
                while (current != 0 && current2 != 0)
                {
                    descriptionbyte.Add(current);
                    descriptionbyte.Add(current2);
                    current = tf.Content[bytesread];
                    bytesread++;
                    current2 = tf.Content[bytesread];
                    bytesread++;
                }
            }
            enc = ID3v23.GetEncoding(_enc);
            this.Description = enc.GetString(descriptionbyte.ToArray());
            bbfr = tf.Content.Skip(bytesread).ToArray();

            this.CoverImage = new Bitmap(new MemoryStream(bbfr));
        }
    }
}
