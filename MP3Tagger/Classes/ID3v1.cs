using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MP3Tagger.Interfaces;
using MP3Tagger.Exceptions;
namespace MP3Tagger.Classes
{
    public class ID3v1 : ID3Tag, IID3v1
    {
        private string _artist,_album,_title,_comments;
        private int _year;
        private int _tracknumber=0;
        private string _genre;
        private static string[] Genres=new string[]{
              "Blues",  "Classic Rock",  "Country",  "Dance",  "Disco",  "Funk",  "Grunge",  "Hip-Hop",  "Jazz",  "Metal", "New Age", "Oldies", "Other", "Pop", "R&B", "Rap", "Reggae", "Rock", "Techno", "Industrial", "Alternative", "Ska", "Death Metal", "Pranks", "Soundtrack", "Euro-Techno", "Ambient", "Trip-Hop", "Vocal", "Jazz+Funk", "Fusion", "Trance", "Classical", "Instrumental", "Acid", "House", "Game", "Sound Clip", "Gospel", "Noise", "AlternRock", "Bass", "Soul", "Punk", "Space", "Meditative", "Instrumental Pop", "Instrumental Rock", "Ethnic", "Gothic", "Darkwave", "Techno-Industrial", "Electronic", "Pop-Folk", "Eurodance", "Dream", "Southern Rock", "Comedy", "Cult", "Gangsta", "Top 40", "Christian Rap", "Pop/Funk", "Jungle", "Native American", "Cabaret", "New Wave", "Psychadelic", "Rave", "Showtunes", "Trailer", "Lo-Fi", "Tribal", "Acid Punk", "Acid Jazz", "Polka", "Retro", "Musical", "Rock & Roll", "Hard Rock",
         "Folk", "Folk-Rock", "National Folk", "Swing", "Fast Fusion", "Bebob", "Latin", "Revival", "Celtic", "Bluegrass", "Avantgarde", "Gothic Rock", "Progressive Rock", "Psychedelic Rock", "Symphonic Rock", "Slow Rock", "Big Band", "Chorus", "Easy Listening", "Acoustic","Humour","Speech","Chanson","Opera","Chamber Music","Sonata","Symphony","Booty Bass","Primus","Porn Groove","Satire","Slow Jam","Club","Tango","Samba","Folklore","Ballad","Power Ballad","Rhythmic Soul","Freestyle","Duet","Punk Rock","Drum Solo","A capella","Euro-House","Dance Hall "};


        public static string GenreFromIndex(int index)
        {
            if (index >= Genres.Length)
            {
                return Genres[12];
            }
            else
            {
                return Genres[index];
            }
        }

        /// <summary>
        /// Return the index for the exact match if found or the index for "other" else, never throws an exception
        /// </summary>
        /// <param name="Genre"></param>
        /// <returns></returns>
        public int IndexFromGenre(string Genre)
        {
            int res = 12;
            for (int k = 0; k < Genres.Length; k++)
            {
                if (Genres[k] == Genre)
                {
                    res = k;
                    break;
                }
            }
            return res;
        }

        public override string Artist
        {
            get
            {
                return _artist;
            }
            set
            {
                if (value.Length > 30)
                {
                    throw new TagLengthWrongException();
                }
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
                if (value.Length > 30)
                {
                    throw new TagLengthWrongException();
                }
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
                if (value.Length > 30)
                {
                    throw new TagLengthWrongException();
                }
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
                    _year = value;
                }
                else
                {
                    throw new TagLengthWrongException();
                }
            }
        }

        public override int Write(FileStream stream)
        {
            long streampos = stream.Position;
            byte[] bytearray;
            try
            {
                stream.Seek(-128, SeekOrigin.End);
                bytearray = new byte[3];
                stream.Read(bytearray, 0, 3);
                if (Encoding.ASCII.GetString(bytearray, 0, 3) != "TAG")
                {
                    stream.Seek(0, SeekOrigin.End);
                }
                else
                {
                    stream.Seek(-128, SeekOrigin.End);
                }
                stream.Write(new byte[] { (byte)'T', (byte)'A', (byte)'G' }, 0, 3);
                stream.Write(StringTo30ByteArray(_title), 0, 30);
                stream.Write(StringTo30ByteArray(_artist), 0, 30);
                stream.Write(StringTo30ByteArray(_album), 0, 30);
                stream.Write(StringTo30ByteArray(_year.ToString()), 0, 4);
                bytearray = StringTo30ByteArray(_comments);
                if (_tracknumber > 0)
                {
                    bytearray[28] = 0;
                    bytearray[29] = (byte)_tracknumber;
                }
                stream.Write(bytearray, 0, 30);
                stream.Write(new byte[] { (byte)IndexFromGenre(_genre) }, 0, 1);
                stream.Seek(streampos, SeekOrigin.Begin);
            }
            catch
            {
                return 0;
            }
            return 1;
        }

        public override bool Read(System.IO.FileStream stream)
        {
            bool res = true;
            byte[] readbfr;
            string tagstring;
            try
            {
                long streampos = stream.Position;
                stream.Seek(-128, SeekOrigin.End);
                readbfr = new byte[3];
                stream.Read(readbfr, 0, 3);
                tagstring = Encoding.ASCII.GetString(readbfr, 0, 3);
                if (tagstring != "TAG")
                {
                    throw new TagNotFoundException();
                }
                readbfr = new byte[30];
                stream.Read(readbfr, 0, 30);
                _title = Encoding.ASCII.GetString(readbfr, 0, 30).TrimEnd((char)0);
                stream.Read(readbfr, 0, 30);
                _artist = Encoding.ASCII.GetString(readbfr, 0, 30).TrimEnd((char)0);
                stream.Read(readbfr, 0, 30);
                _album = Encoding.ASCII.GetString(readbfr, 0, 30).TrimEnd((char)0);
                readbfr = new byte[4];
                stream.Read(readbfr, 0, 4);
                _year = int.Parse(Encoding.ASCII.GetString(readbfr, 0, 4));
                readbfr = new byte[30];
                stream.Read(readbfr, 0, 30);
                if (readbfr[28] == 0 && readbfr[29] != 0) // track number present as last bit of comment
                {
                    _tracknumber = (int)readbfr[29];
                    _comments = Encoding.ASCII.GetString(readbfr, 0, 28).TrimEnd((char)0);
                }
                else
                {
                    _tracknumber = 0;
                    _comments = Encoding.ASCII.GetString(readbfr, 0, 30).TrimEnd((char)0);
                }
                int genre = stream.ReadByte();
                _genre = GenreFromIndex(genre);
                stream.Seek(streampos, SeekOrigin.Begin);
            }
            catch 
            {
                res = false;
            }
            return res;
        }


        public override string Comments
        {
            get
            {
                return _comments;
            }
            set
            {
                if (value.Length > 30)
                {
                    throw new TagLengthWrongException();
                }
                _comments = value;
            }
        }

        private byte[] StringTo30ByteArray(string input)
        {
            byte[] res,bfr;
            res = new byte[30];
            bfr = Encoding.ASCII.GetBytes(input);
            for (int k = 0; k < input.Length; k++)
            {
                res[k] = bfr[k];
            }
            return res;
        }


        public override string Genre
        {
            get
            {
                return _genre;
            }
            set
            {
                _genre = value;
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
                if (_tracknumber > -1)
                {
                    _tracknumber = value;
                }
            }
        }
    }
}
