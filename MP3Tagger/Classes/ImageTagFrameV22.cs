using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
namespace MP3Tagger.Classes
{
    public class ImageTagFrameV22 : TagFrameV22
    {
        private string _imageformat="JPG"; // jpeg as default
        private string _descr = "";
        private byte _pictype = 3; // front cover as default
        private byte[] _enc;
        private Image _img;
        public string ImageFormat
        {
            get
            {
                return _imageformat;
            }
            set
            {
                _imageformat = value;
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
            uint framesize = 0; // string terminations, encoding and picture type
            System.Text.Encoding encod;
            List<byte> totalcontent = new List<byte>();
            encod = ID3v22.GetEncoding(_enc);
            if (_enc[0] == 0)
            {
                framesize = 3;
            }
            else
            {
                framesize = 4;
            }
            byte[] mime,desc;
            mime=System.Text.Encoding.ASCII.GetBytes(ImageFormat);
            desc=encod.GetBytes(Description);
            framesize += (uint)mime.Length;
            framesize += (uint)desc.Length;
            memstream = new MemoryStream(2000000);
            if(ImageFormat=="JPG")
            {
                imgfmt=System.Drawing.Imaging.ImageFormat.Jpeg;
            }
            else if (ImageFormat=="PNG")
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
            outstream.Write(bbfr, 0, 3);
            outstream.Write(BitConverter.GetBytes(framesize).Reverse().ToArray(), 0, 3);
            outstream.WriteByte(_enc[0]);
            outstream.Write(mime, 0, mime.Length);
            outstream.WriteByte(PictureType);
            outstream.Write(desc, 0, desc.Length);
            outstream.WriteByte(0);
            if (_enc[0] == 1)
            {
                outstream.WriteByte(0);
            }
            outstream.Write(memstream.ToArray(), 0, (int)memstream.Length);
            memstream.Close();

        }

        public ImageTagFrameV22()
        {
            _enc = new byte[]{0};
            _imageformat = "JPG";
            _pictype = 3;
        }

        public ImageTagFrameV22(TagFrameV22 tf)
        {
            Encoding enc;
            byte current,current2;
            int bytesread = 0;
            List<byte> mimetypebyte;
            List<byte> descriptionbyte;
            byte[] bbfr;
            this.Content = tf.Content;
            this.FrameHeader = tf.FrameHeader;
            this.Encoding = tf.Content.Take(3).ToArray();
            bytesread++;
            current = tf.Content[bytesread];
            bytesread += 3; 

            mimetypebyte = new List<byte>();
            while (current != 0)
            {
                mimetypebyte.Add(current);
                current = tf.Content[bytesread];
                bytesread++;
            }
            this.ImageFormat = System.Text.Encoding.ASCII.GetString(Content.Skip(1).Take(3).ToArray()); //mime type encoding is always ascii
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
                _enc[2] = current;
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
            enc = ID3v22.GetEncoding(_enc);
            this.Description = enc.GetString(descriptionbyte.ToArray());
            bbfr = tf.Content.Skip(bytesread).ToArray();

            this.CoverImage = new Bitmap(new MemoryStream(bbfr));
        }
    }
}
