using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Sockets;
using CDRipperLib;
using MP3Tagger.Interfaces;
using MP3Tagger.Classes;

namespace CDDBAccess
{
    public class CDDBConnection
    {
        private string _hostname = "freedb.freedb.org";
        private int _port = 8880;
        private string _username = "labbear";
        private string _clienthost = "lbelectronics";
        private string _product = "musigtopf";
        private string _version = "v1";
        private TcpClient _client;
        private NetworkStream _netstream;

        public CDDBConnection()
        {
            _client = new TcpClient();
        }

        ~CDDBConnection()
        {
            try
            {
                _netstream.Close();
                _client.Close();
            }
            catch
            { 
            }
        }


        public CDDBEntry ReadCD(CDDBEntry entry)
        {
            CDDBEntry res = null;
            string cmdres;
            string[] splitted;
            if (Connect() == 200)
            {
                cmdres = SendCommand("cddb read " + entry.Category + " " + entry.DiscID,false);
                if (cmdres.Substring(0, 3) == "210")
                {
                    res = new CDDBEntry();
                    splitted = cmdres.Split('\n')[0].Split(' ');
                    res.Category = splitted[1];
                    res.DiscID = splitted[2];
                    cmdres = cmdres.Replace(res.Category, "").Replace(res.DiscID, "").Substring(3);
                    res.Content = cmdres;
                }
            }
            return res;
        }

        public List<ID3Tag> ReadCD(CDDBEntry discidcontainer, MP3Tagger.TagTypeEnum tagtype,out string extdata)
        {
            string content,bfr;
            string[] splitted;
            string globalbum=null, globartist=null;
            int globyear=0;
            string globgenre=null;
            ID3Tag tag;
            List<ID3Tag> res = new List<ID3Tag>();
            CDDBEntry entry = ReadCD(discidcontainer);
            extdata = "";


            if(entry!=null)
            {
                content = entry.Content;
                splitted = content.Split('\n');
                for (int z = 0; z < splitted.Length; z++)
                {
                    if (splitted[z].Length > 1)
                    {
                        bfr = splitted[z].TrimEnd('\r');
                        if (splitted[z].StartsWith("DTITLE"))
                        {
                            Match m;
                            bfr = bfr.Replace("DTITLE=", "");
                            m = Regex.Match(bfr, "\\s/\\s");
                            if (m.Success)
                            {
                                globartist = bfr.Substring(0, m.Index).Trim();
                                globalbum=bfr.Substring(m.Index + 2).Trim();
                            }
                            else
                            {
                                globartist = bfr.Trim();
                                globalbum = bfr.Trim();
                            }
                        }
                        else if (splitted[z].StartsWith("DYEAR"))
                        {
                            bfr = bfr.Replace("DYEAR=", "");
                            globyear = Convert.ToInt32(bfr);
                        }
                        else if (splitted[z].StartsWith("DGENRE"))
                        {
                            bfr = bfr.Replace("DGENRE=", "");
                            globgenre = bfr.Trim();
                        }
                        else if (splitted[z].StartsWith("TTITLE"))
                        {
                            if (tagtype == MP3Tagger.TagTypeEnum.ID3v1)
                            {
                                tag = new MP3Tagger.Classes.ID3v1();
                            }
                            else
                            {
                                tag = new MP3Tagger.Classes.ID3v23();
                            }
                            MatchCollection m;
                            bfr = bfr.Replace("TTITLE", "");
                            tag.TrackNumber = Convert.ToInt32(bfr.Substring(0, bfr.IndexOf('='))) + 1;
                            bfr = bfr.Substring(bfr.IndexOf('=')+1);
                            m = Regex.Matches(bfr, "\\s/\\s");
                            if (m.Count == 1)
                            {
                                tag.Artist = bfr.Substring(0, m[0].Index).Trim();
                                tag.Title = bfr.Substring(m[0].Index + 2).Trim();
                            }
                            else
                            {
                                tag.Title = bfr.Trim();
                            }
                            res.Add(tag);
                        }
                        else if (splitted[z].StartsWith("EXTD"))
                        {
                            extdata += bfr.Replace("EXTD=", "");
                        }
                    }
                }
                foreach (IID3Tag tg in res)
                {
                    if (globalbum != null)
                    {
                        tg.Album = globalbum;
                    }
                    if (globartist != null && tg.Artist == null)
                    {
                        tg.Artist = globartist;
                    }
                    if (globyear > 0)
                    {
                        tg.Year = globyear;
                    }
                    if (globgenre != null)
                    {
                        tg.Genre = globgenre;
                    }

                }
                extdata = Regex.Replace(extdata, "\\\\n", "\r\n");
        }
            return res;
        }

        public CDDBEntry[] QueryCD(CDRipperLib.CDROM_TOC toc)
        {
            string res = "";
            string querystring;
            string[] splitted,splitted2;
            CDDBEntry[] result=null;
            if (Connect() == 200)
            {
                querystring = "cddb query " + toc.GetDiscID() + " " + toc.LastTrack;
                for (int k = 0; k < toc.LastTrack; k++)
                {
                    querystring += " " + (toc.GetTrackData(k).StartSector + 150).ToString();
                }
                querystring += " " + ((toc.GetTrackData(toc.LastTrack).StartSector - toc.GetTrackData(0).StartSector) / 75);
                res = SendCommand(querystring,false);
                if (res.Substring(0, 3) == "200")
                {
                    splitted = res.Split(' ');
                    res = "";
                    result = new CDDBEntry[1];
                    result[0] = new CDDBEntry();
                    result[0].Category = splitted[1];
                    result[0].DiscID = splitted[2];
                    for (int k = 3; k < splitted.Length; k++)
                    {
                        res += splitted[k] + " ";
                    }
                    result[0].Content = res.Trim(' ');
                }
                else if (res.Substring(0, 3) == "211")
                {
                    splitted = res.Split('\n');
                    res = "";
                    result = new CDDBEntry[splitted.Length - 2];
                    for (int u = 1; u < splitted.Length-1; u++)
                    {
                        splitted2 = splitted[u].Split(' ');
                        res = "";
                        result[u - 1] = new CDDBEntry();
                        result[u - 1].Category = splitted2[0];
                        result[u - 1].DiscID = splitted2[1];
                        for (int k = 2; k < splitted2.Length; k++)
                        {
                            res += splitted2[k] + " ";
                        }
                        result[u - 1].Content = res.Trim(' ');
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// connects and does the handshake, should return 200, if not the handshake or connection failed.
        /// </summary>
        /// <returns></returns>
        private int Connect()
        {
            string handshakestring,res;
            List<byte> result=new List<byte>();
            byte[] initres=new byte[512];
            handshakestring="cddb hello " + _username + " " + _clienthost + " " + _product + " " + _version;
            if (!_client.Connected)
            {
                try
                {
                    _client.Connect(_hostname, _port);
                    _netstream = _client.GetStream();
                    int bytesread = _netstream.Read(initres, 0, 512);
                    res = SendCommand(handshakestring,true);
                    return Convert.ToInt32(res.Substring(0, 3));
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return 200;
            }
        }

        public string SendCustomCommand(string cmd)
        {
            string res = "";
            if (Connect() == 200)
            {
                res = SendCommand(cmd,false);
            }
            return res;
        }

        private string SendCommand(string cmd,bool onelineresp)
        {
            List<byte> result = new List<byte>();
            byte currbyte = 0,prevbyte=0;
            bool commented = false;
            _netstream.Write(Encoding.GetEncoding("ISO-8859-1").GetBytes(cmd + "\r\n"), 0, Encoding.GetEncoding("ISO-8859-1").GetByteCount(cmd + "\r\n"));
            while (!(currbyte == 46 && prevbyte == 10) || commented) // while not endsign unescaped
            {

                prevbyte = currbyte;
                currbyte = (byte)_netstream.ReadByte();
                if (onelineresp)
                {
                    if (currbyte == 10 || currbyte==13)
                    {
                        currbyte = 46;
                        prevbyte = 10;
                        _netstream.ReadByte();
                    }
                }
                if (prevbyte == 10)
                {
                    if (currbyte == 35) // comment line encountered
                    {
                        commented = true;
                    }
                    else
                    {
                        commented = false;
                    }
                }
                result.Add(currbyte);
            }
            if (!onelineresp)
            {
                _netstream.ReadByte(); // read carriage return and newline
                _netstream.ReadByte();
            }
            return Encoding.GetEncoding("ISO-8859-1").GetString(result.ToArray());
        }
    }
}
