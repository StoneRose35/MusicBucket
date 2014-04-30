using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using MP3Tagger.Interfaces;
using MP3Tagger.Exceptions;
using System.Globalization;
namespace MP3Tagger.Classes
{
    /// <summary>
    /// Class representing and id3 tag of version 2.2.0 
    /// </summary>
    public class ID3v22 : ID3Tag, IID3v22
    {
        private string _artist, _album, _title, _comments;
        private int _year;
        private int _tracknumber;
        private string _genre;
        private Image _frontcover;
        private List<TagFrameV22> _frames;
        private byte[] _extheadersize, _extheaderflags, _paddingsize, _extheadercontent = null;
        private byte _headerflags = 0;
        private uint _tagsize = 0;
        private int _extendedheadersize = -1;
        private int _artistIndex = -1, _artist2Index = -1, _albumIndex = -1, _titleIndex = -1, _commmentsIndex = -1, _yearIndex = -1, _tracknumberIndex = -1, _genreIndex = -1, _frontcoverIndex = -1;
        private const string ARTIST = "TP1";
        private const string ARTIST2 = "TP2";
        private const string ALBUM = "TAL";
        private const string TITLE = "TT2";
        private const string YEAR = "TYE";
        private const string TRACK = "TRK";
        private const string GENRE = "TCO";
        private const string COVER = "PIC";
        private const string COMMENTS = "COM";

        public ID3v22()
        {
            _frames = new List<TagFrameV22>();
        }
        public override string Artist
        {
            get
            {
                return _artist;
            }
            set
            {
                TagFrameV22 frame;
                Encoding enc;
                byte[] _content;
                if (_artistIndex >= 0)
                {
                    frame = _frames[_artistIndex];
                    enc = GetEncoding(frame.Content);
                    _content = enc.GetBytes(value);
                    if (frame.Content[0] == 0)
                    {
                        _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0 }).ToArray(); // take encoding defined previously
                    }
                    else
                    {
                        _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0,0 }).ToArray(); // take encoding defined previously
                    }
                }
                else
                {
                    frame = new TagFrameV22();
                    frame.FrameHeader = ARTIST2;
                    enc = new UnicodeBOM();
                    _content = enc.GetBytes(value);
                    _content = (new byte[] { 1 }).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // encode in unicode for new text tags
                    _artistIndex = _frames.Count;
                    _frames.Add(frame);
                }
                frame.Content = _content;

                if (_artist2Index >= 0)
                {
                    frame = _frames[_artist2Index];
                    enc = GetEncoding(frame.Content);
                    _content = enc.GetBytes(value);
                    if (frame.Content[0] == 0)
                    {
                        _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0 }).ToArray(); // take encoding defined previously
                    }
                    else
                    {
                        _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // take encoding defined previously
                    }
                }
                else
                {
                    frame = new TagFrameV22();
                    frame.FrameHeader = ARTIST;
                    enc = new UnicodeBOM();
                    _content = enc.GetBytes(value);
                    _content = (new byte[] { 1 }).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // encode in unicode for new text tags
                    _artist2Index = _frames.Count;
                    _frames.Add(frame);
                }

                frame.Content = _content;
                _artist = value;
            }
        }

        public override string Album
        {
            get
            {
                return _album;
            }
            set
            {
                TagFrameV22 frame;
                Encoding enc;
                byte[] _content;
                if (_albumIndex >= 0)
                {
                    frame = _frames[_albumIndex];
                    enc = GetEncoding(frame.Content);
                    _content = enc.GetBytes(value);
                    if (frame.Content[0] == 0)
                    {
                        _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0 }).ToArray(); // take encoding defined previously
                    }
                    else
                    {
                        _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // take encoding defined previously
                    }
                }
                else
                {
                    frame = new TagFrameV22();
                    frame.FrameHeader = ALBUM;
                    enc = new UnicodeBOM();
                    _content = enc.GetBytes(value);
                    _content = (new byte[] { 1 }).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // encode in unicode for new text tags
                    _albumIndex = _frames.Count;
                    _frames.Add(frame);
                }

                frame.Content = _content;

                _album = value;
            }
        }

        public override string Title
        {
            get
            {
                return _title;
            }
            set
            {
                TagFrameV22 frame;
                Encoding enc;
                byte[] _content;
                if (_titleIndex >= 0)
                {
                    frame = _frames[_titleIndex];
                    enc = GetEncoding(frame.Content);
                    _content = enc.GetBytes(value);
                    if (frame.Content[0] == 0)
                    {
                        _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0 }).ToArray(); // take encoding defined previously
                    }
                    else
                    {
                        _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // take encoding defined previously
                    }
                }
                else
                {
                    frame = new TagFrameV22();
                    frame.FrameHeader = TITLE;
                    enc = new UnicodeBOM();
                    _content = enc.GetBytes(value);
                    _content = (new byte[] { 1 }).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // encode in unicode for new text tags
                    _titleIndex = _frames.Count;
                    _frames.Add(frame);
                }

                frame.Content = _content;
                _title = value;
            }
        }

        public override int Year
        {
            get
            {
                return _year;
            }
            set
            {
                if (value >= 0 && value < 10000)
                {
                    string strvalue;
                    TagFrameV22 frame;
                    Encoding enc;
                    byte[] _content;
                    strvalue = value.ToString();
                    if (_yearIndex >= 0)
                    {
                        frame = _frames[_yearIndex];
                        enc = GetEncoding(frame.Content);
                        _content = enc.GetBytes(strvalue);
                        if (frame.Content[0] == 0)
                        {
                            _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0 }).ToArray(); // take encoding defined previously
                        }
                        else
                        {
                            _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // take encoding defined previously
                        }
                    }
                    else
                    {
                        frame = new TagFrameV22();
                        frame.FrameHeader = YEAR;
                        enc = new UnicodeBOM();
                        _content = enc.GetBytes(strvalue);
                        _content = (new byte[] { 1 }).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // encode in unicode for new text tags
                        _yearIndex = _frames.Count;
                        _frames.Add(frame);
                    }

                    frame.Content = _content;
                    _year = value;
                }
                else
                {
                    throw new TagLengthWrongException();
                }
            }
        }

        public override string Comments
        {
            get
            {
                return _comments;
            }
            set
            {
                CommentTagFrameV22 cmtTF;
                Encoding enc;
                if (_commmentsIndex >= 0)
                {
                    cmtTF = _frames[_frontcoverIndex] as CommentTagFrameV22;
                }
                else
                {
                    cmtTF = new CommentTagFrameV22();
                    cmtTF.FrameHeader = COMMENTS;
                    enc = new UnicodeBOM();
                    _commmentsIndex = _frames.Count;
                    _frames.Add(cmtTF);
                }

                cmtTF.MainComment = value;
                _comments = value;
            }
        }

        public override string Genre
        {
            get
            {
                return _genre;
            }
            set
            {
                TagFrameV22 frame;
                Encoding enc;
                byte[] _content;
                if (_genreIndex >= 0)
                {
                    frame = _frames[_genreIndex];
                    enc = GetEncoding(frame.Content);
                    _content = enc.GetBytes(value);
                    if (frame.Content[0] == 0)
                    {
                        _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0 }).ToArray(); // take encoding defined previously
                    }
                    else
                    {
                        _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // take encoding defined previously
                    }
                }
                else
                {
                    frame = new TagFrameV22();
                    frame.FrameHeader = GENRE;
                    enc = new UnicodeBOM();
                    _content = enc.GetBytes(value);
                    _content = (new byte[] { 1 }).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // encode in unicode for new text tags
                    _genreIndex = _frames.Count;
                    _frames.Add(frame);
                }

                frame.Content = _content;
                _genre = value;
            }
        }

        public System.Drawing.Image FrontCover
        {
            get
            {
                return _frontcover;
            }
            set
            {
                ImageTagFrameV22 imgTF;
                _frontcover = value;
                if (_frontcoverIndex >= 0)
                {
                    imgTF = _frames[_frontcoverIndex] as ImageTagFrameV22;
                }
                else
                {
                    imgTF = new ImageTagFrameV22();
                    imgTF.FrameHeader = COVER;
                    _frontcoverIndex = _frames.Count;
                    _frames.Add(imgTF as TagFrameV22);
                }
                imgTF.CoverImage = _frontcover;
            }
        }

        public override int TrackNumber
        {
            get
            {
                return _tracknumber;
            }
            set
            {
                if (value >= 0)
                {
                    TagFrameV22 frame;
                    Encoding enc;
                    byte[] _content;
                    string strvalue = value.ToString();
                    if (_tracknumberIndex >= 0)
                    {
                        frame = _frames[_tracknumberIndex];
                        enc = GetEncoding(frame.Content);
                        _content = enc.GetBytes(strvalue);
                        if (frame.Content[0] == 0)
                        {
                            _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0 }).ToArray(); // take encoding defined previously
                        }
                        else
                        {
                            _content = (frame.Content.Take(1)).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // take encoding defined previously
                        }
                    }
                    else
                    {
                        frame = new TagFrameV22();
                        frame.FrameHeader = TRACK;
                        enc = new UnicodeBOM();
                        _content = enc.GetBytes(strvalue);
                        _content = (new byte[] { 1 }).Concat(_content).Concat(new byte[] { 0, 0 }).ToArray(); // encode in unicode for new text tags
                        _tracknumberIndex = _frames.Count;
                        _frames.Add(frame);
                    }

                    frame.Content = _content;
                    _tracknumber = value;
                }
                else
                {
                    throw new TagLengthWrongException();
                }
            }
        }

        public override int Write(System.IO.Stream stream)
        {
            long currpos;
            currpos = stream.Position;
            string strbfr;
            byte[] bbfr;
            bbfr = new byte[3];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bbfr, 0, 3);
            strbfr = Encoding.ASCII.GetString(bbfr);
            int comptagsize = 10;

            // compute tag size
            foreach (TagFrameV22 tf in _frames)
            {
                comptagsize += (int)tf.FrameSize + 6;
            }
            if (comptagsize > _tagsize) // add padding
            {
                int mp3size;
                mp3size = (int)(stream.Length - _tagsize);
                stream.Seek(_tagsize, SeekOrigin.Begin);
                bbfr = new byte[stream.Length - _tagsize];
                stream.Read(bbfr, 0, mp3size);
                if (comptagsize - _tagsize > 8196)
                {
                    _tagsize += (uint)(comptagsize - _tagsize);
                }
                else
                {
                    _tagsize += 8196;
                }

                stream.Seek(_tagsize, SeekOrigin.Begin);
                stream.Write(bbfr, 0, mp3size);
            }
            else if (_tagsize - comptagsize > 8196) // remove padding
            {
                int mp3size;
                mp3size = (int)(stream.Length - _tagsize);
                stream.Seek(_tagsize, SeekOrigin.Begin);
                bbfr = new byte[stream.Length - _tagsize];
                stream.Read(bbfr, 0, mp3size);
                _tagsize = (uint)comptagsize + 8196;
                stream.Seek(_tagsize, SeekOrigin.Begin);
                stream.Write(bbfr, 0, mp3size);
                stream.SetLength(_tagsize + mp3size);
            }
            bbfr = new byte[_tagsize]; // write zeros
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(bbfr, 0, (int)_tagsize);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(Encoding.ASCII.GetBytes("ID3"), 0, 3);
            stream.WriteByte(2);
            stream.WriteByte(0);
            stream.WriteByte(_headerflags);
            stream.Write(EncodeTagSize(_tagsize), 0, 4);
            foreach (TagFrameV22 tf in _frames)
            {
                tf.Save(stream);
            }
            stream.Seek(currpos, SeekOrigin.Begin);
            return 0;
        }

        public override bool Read(System.IO.Stream stream,bool strict=false)
        {
            bool res = true;
            byte[] bbfr;


            int framecnt = 0;
            bool lastframe = false;
            string strbfr, framename;
            Encoding enc;
            TagFrameV22 frame;
            long streampos = stream.Position;
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            bbfr = new byte[3];
            stream.Read(bbfr, 0, 3);
            strbfr = Encoding.ASCII.GetString(bbfr);
            if (strbfr != "ID3")
            {
                res = false;
                if (strict)
                {
                    throw new TagNotFoundException();
                }
            }
            bbfr = new byte[2];
            stream.Read(bbfr, 0, 2);
            if (!(bbfr[0] == 2 && bbfr[1] == 0))
            {
                res = false;
                if (strict)
                {
                    throw new TagWrongVersionException();
                }
            }
            _headerflags = (byte)stream.ReadByte();
            bbfr = new byte[4];
            stream.Read(bbfr, 0, 4);
            _tagsize = (uint)(bbfr[0] << 21 | bbfr[1] << 14 | bbfr[2] << 7 | bbfr[3]);


            // READ FRAMES
            while (stream.Position < _tagsize + 10 && !lastframe)
            {

                    frame = new TagFrameV22();
                    frame.Read(stream);
                    framename = IsTagKnown(frame.FrameHeader);

                    switch (framename)
                    {
                        case "":
                            break;
                        case ARTIST:
                            enc = GetEncoding(frame.Content);
                            _artistIndex = framecnt;
                            _artist = enc.GetString(frame.Content.Skip(1).ToArray()).TrimEnd(new char[] { (char)0 });
                            break;
                        case ALBUM:
                            enc = GetEncoding(frame.Content);
                            _albumIndex = framecnt;
                            _album = enc.GetString(frame.Content.Skip(1).ToArray()).TrimEnd(new char[] { (char)0 });
                            break;
                        case TITLE:
                            enc = GetEncoding(frame.Content);
                            _titleIndex = framecnt;
                            _title = enc.GetString(frame.Content.Skip(1).ToArray()).TrimEnd(new char[] { (char)0 });
                            break;
                        case YEAR:
                            enc = GetEncoding(frame.Content);
                            _yearIndex = framecnt;
                            int.TryParse(enc.GetString(frame.Content.Skip(1).ToArray()), out _year);
                            break;
                        case TRACK:
                            enc = GetEncoding(frame.Content);
                            _tracknumberIndex = framecnt;
                            int.TryParse(enc.GetString(frame.Content.Skip(1).ToArray()).Split('/')[0], out _tracknumber);
                            break;
                        case GENRE:
                            enc = GetEncoding(frame.Content);
                            _genreIndex = framecnt;
                            _genre = DecodeGenre(enc.GetString(frame.Content.Skip(1).ToArray()).TrimEnd(new char[] { (char)0 }));
                            break;
                        case COVER:
                            ImageTagFrameV22 imgtf = new ImageTagFrameV22(frame);
                            _frontcoverIndex = framecnt;
                            _frontcover = imgtf.CoverImage;
                            frame = imgtf;
                            break;
                        case COMMENTS:
                            CommentTagFrameV22 cmtTF = new CommentTagFrameV22(frame);
                            _commmentsIndex = framecnt;
                            _comments = cmtTF.MainComment;
                            frame = cmtTF;
                            break;
                    }



                if (frame.FrameSize == 0)
                {
                    lastframe = true;
                }
                else
                {
                    _frames.Add(frame);
                    framecnt++;
                }

            }
            stream.Seek(streampos, System.IO.SeekOrigin.Begin);
            return res;
        }


        public List<TagFrameV22> Frames
        {
            get
            {
                return _frames;
            }
        }
        bool IID3v22.UnsynchronizationUsed
        {
            get { return (_headerflags & 128) != 0; }
        }

        bool IID3v22.CompressionUsed
        {
            get { return (_headerflags & 64) != 0; }
        }

        #region private members

        private string IsTagKnown(byte[] tagheader)
        {

            string tagstring = Encoding.ASCII.GetString(tagheader);
            return IsTagKnown(tagstring);
        }

        private string IsTagKnown(string tagheader)
        {
            string tagstring = tagheader;
            if (tagstring == ARTIST || tagstring == ARTIST2)
            {
                return ARTIST;
            }
            else if (tagstring == ALBUM)
            {
                return ALBUM;
            }
            else if (tagstring == TITLE)
            {
                return TITLE;
            }
            else if (tagstring == YEAR)
            {
                return YEAR;
            }
            else if (tagstring == TRACK)
            {
                return TRACK;
            }
            else if (tagstring == GENRE)
            {
                return GENRE;
            }
            else if (tagstring == COVER)
            {
                return COVER;
            }
            else if (tagstring == COMMENTS)
            {
                return COMMENTS;
            }
            else
            {
                return "";
            }
        }

        private string DecodeGenre(string instr)
        {
            string outstr = "";
            outstr = instr;
            string subgenre;
            MatchCollection matches;
            Regex regex = new Regex("\\([0-9]{1,3}\\)");
            matches = regex.Matches(instr);
            foreach (Match m in matches)
            {
                subgenre = ID3v1.GenreFromIndex(Convert.ToInt32(m.Value.TrimStart('(').TrimEnd(')')));
                outstr.Replace(m.Value, subgenre + ",");
            }
            return outstr;
        }

        public static Encoding GetEncoding(byte[] content)
        {
            Encoding enc;
            if (content[0] == 0)
            {
                enc = Encoding.GetEncoding("ISO-8859-1");
            }
            else
            {
                if (content[0] == 1 && content[1] == 255 && content[2] == 254)
                {
                    enc = new UnicodeBOM(false);
                }
                else if (content[0] == 1 && content[1] == 254 && content[2] == 255)
                {
                    enc = new UnicodeBOM(true);
                }
                else
                {
                    enc = Encoding.Unicode;
                }
            }
            return enc;
        }

        private byte[] EncodeTagSize(uint tagsize)
        {
            byte[] res = new byte[4];
            res[0] = (byte)((tagsize >> 21) & 127);
            res[1] = (byte)((tagsize >> 14) & 127);
            res[2] = (byte)((tagsize >> 7) & 127);
            res[3] = (byte)(tagsize & 127);
            return res;
        }

        #endregion


    }
}
