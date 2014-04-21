using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using PortableDeviceApiLib;
using System.Runtime.InteropServices;
namespace PTCAccess
{
    public class PTCWrapper
    {
        private PortableDeviceManager _manager;
        private const uint NUMBER_OBJECTS = 10;
        public PTCWrapper()
        {
            _manager = new PortableDeviceManager();
        }

        public List<PTCDevice> GetDevices()
        { 
            List<PTCDevice> devices=new List<PTCDevice>();
            uint ndevices=0;
            string[] devicesStr;
            string friendlyName=null;
            uint nbytes=0;
            _manager.GetDevices(null,ref ndevices);
            if (ndevices > 0)
            {
                devicesStr = new string[ndevices];
                _manager.GetDevices(devicesStr, ref ndevices);
                foreach (string dev in devicesStr) // get clear text name of each device
                {
                    _manager.GetDeviceFriendlyName(dev, null, ref nbytes);
                    friendlyName = new string((char)0, (int)(nbytes-1));
                    _manager.GetDeviceFriendlyName(dev, friendlyName, ref nbytes);
                    devices.Add(new PTCDevice() { ID = dev, Name = friendlyName });
                }
            }
            return devices;
        }

        public IPortableDevice OpenDevice(string deviceID)
        {
            IPortableDevice res;
            PortableDeviceTypesLib._tagpropertykey test;
            PortableDeviceTypesLib.PortableDeviceValues vals;
            vals = new PortableDeviceTypesLib.PortableDeviceValues();
            res = new PortableDeviceFTM();
            test = GetPropertyKey("WPD_CLIENT_NAME");
            vals.SetStringValue(GetPropertyKey("WPD_CLIENT_NAME"), "MusicBucket");
            vals.SetUnsignedIntegerValue(GetPropertyKey("WPD_CLIENT_MAJOR_VERSION"), 0);
            vals.SetUnsignedIntegerValue(GetPropertyKey("WPD_CLIENT_MINOR_VERSION"), 1);
            vals.SetUnsignedIntegerValue(GetPropertyKey("WPD_CLIENT_REVISION"), 42);
            res.Open(deviceID,(PortableDeviceApiLib.IPortableDeviceValues)vals);
            return res;
        }

        public List<PTCFolder> GetFolders(string parentFolder,string deviceID)
        {
            IPortableDevice dev;
            IPortableDeviceContent cnt;
            IEnumPortableDeviceObjectIDs oids;
            IPortableDeviceProperties props;
            IPortableDeviceValues propVals;
            IPortableDeviceKeyCollection keyColl;
            PTCFolder parent=null;
            Guid contentType;
            uint fetched=NUMBER_OBJECTS;
            string[] bfr;
            string fname, fuid;
            List<PTCFolder> res = new List<PTCFolder>();
            dev = this.OpenDevice(deviceID);
            dev.Content(out cnt);
            if(parentFolder==null)
            {
                parentFolder="DEVICE";
            }
            cnt.EnumObjects(0, parentFolder, null, out oids);
            cnt.Properties(out props);
            bfr = new string[NUMBER_OBJECTS];
            keyColl = (IPortableDeviceKeyCollection)(new PortableDeviceTypesLib.PortableDeviceKeyCollection());
            keyColl.Add(GetApiPropertyKey("WPD_OBJECT_NAME"));
            keyColl.Add(GetApiPropertyKey("WPD_OBJECT_PERSISTENT_UNIQUE_ID"));
            keyColl.Add(GetApiPropertyKey("WPD_OBJECT_CONTENT_TYPE"));

            // get current (parent) folder info
            props.GetValues(parentFolder, keyColl, out propVals);
            propVals.GetGuidValue(GetApiPropertyKey("WPD_OBJECT_CONTENT_TYPE"), out contentType);
            propVals.GetStringValue(GetApiPropertyKey("WPD_OBJECT_NAME"), out fname);
            propVals.GetStringValue(GetApiPropertyKey("WPD_OBJECT_PERSISTENT_UNIQUE_ID"), out fuid);
            parent = new PTCFolder() { Id = parentFolder, Name = fname, Uid = fuid, IsRootFolder = false, DeviceID = deviceID };


            while (fetched == NUMBER_OBJECTS)
            {
                oids.Next(NUMBER_OBJECTS, bfr, ref fetched);
                for (int K = 0; K < fetched; K++)
                {
                    props.GetValues(bfr[K], keyColl, out propVals);
                    propVals.GetGuidValue(GetApiPropertyKey("WPD_OBJECT_CONTENT_TYPE"), out contentType);
                    propVals.GetStringValue(GetApiPropertyKey("WPD_OBJECT_NAME"), out fname);
                    propVals.GetStringValue(GetApiPropertyKey("WPD_OBJECT_PERSISTENT_UNIQUE_ID"), out fuid);
                    if (contentType == GetContentTypeGuid("WPD_CONTENT_TYPE_FOLDER"))
                    {
                        res.Add(new PTCFolder() { Id=bfr[K],Name=fname,Uid=fuid,IsRootFolder=false,DeviceID=deviceID,Parent=parent});
                    }
                    else if (contentType == GetContentTypeGuid("WPD_CONTENT_TYPE_FUNCTIONAL_OBJECT"))
                    {
                        if(GetFolders(bfr[K],deviceID).Count>0)
                        {
                            res.Add(new PTCFolder() { Id = bfr[K], Name = fname, Uid = fuid,IsRootFolder=true,DeviceID=deviceID,Parent=parent });
                        }
                    }
                }
            }
            
            return res;
        }

        public List<string> GetMp3Files(string folderID, string deviceID)
        {
            IPortableDevice dev;
            IPortableDeviceContent content;
            IPortableDeviceResources resources;
            IPortableDeviceProperties props;
            IPortableDeviceKeyCollection keyColl;
            IPortableDeviceValues vals;
            IEnumPortableDeviceObjectIDs oids;
            MemoryStream fstream;
            IntPtr bread;
            Guid contentType;
            string filename;
            
            IStream stream;
            uint optimalbuffersize=0;
            byte[] bytearray;
            uint bytesread;
            string[] bfr;
            uint fetched = NUMBER_OBJECTS;
            keyColl=(new PortableDeviceTypesLib.PortableDeviceKeyCollection()) as IPortableDeviceKeyCollection;
            bfr = new string[NUMBER_OBJECTS];
            List<string> res = new List<string>();
            dev = this.OpenDevice(deviceID);
            dev.Content(out content);
            content.Properties(out props);
            keyColl.Add(GetApiPropertyKey("WPD_OBJECT_ORIGINAL_FILE_NAME"));
            keyColl.Add(GetApiPropertyKey("WPD_OBJECT_CONTENT_TYPE"));
            content.EnumObjects(0, folderID, null, out oids);
            content.Transfer(out resources);
            while (fetched == NUMBER_OBJECTS)
            {
                oids.Next(NUMBER_OBJECTS, bfr, ref fetched);
                for (int K = 0; K < fetched; K++)
                {
                    props.GetValues(bfr[K], keyColl, out vals);
                    vals.GetStringValue(GetApiPropertyKey("WPD_OBJECT_ORIGINAL_FILE_NAME"), out filename);
                    vals.GetGuidValue(GetApiPropertyKey("WPD_OBJECT_CONTENT_TYPE"), out contentType);
                    if (contentType == GetContentTypeGuid("WPD_CONTENT_TYPE_AUDIO"))
                    {
                        res.Add(filename);
                        resources.GetStream(bfr[K], GetApiPropertyKey("WPD_RESOURCE_DEFAULT"), 0, ref optimalbuffersize, out stream);
                        bytearray = new byte[optimalbuffersize];
                        bytesread = optimalbuffersize;
                        fstream = new MemoryStream(); //FileStream("C:\\Users\\fuerh_000\\Music\\" + filename,FileMode.Create);
                        bread = Marshal.AllocHGlobal(sizeof(ulong));
                        while (bytesread == optimalbuffersize)
                        {
                            ((System.Runtime.InteropServices.ComTypes.IStream)stream).Read(bytearray, (int)optimalbuffersize, bread);
                            bytesread=(uint)Marshal.ReadInt64(bread);
                            fstream.Write(bytearray,0,(int)bytesread);
                        }
                        Marshal.FreeHGlobal(bread);
                        fstream.Write(bytearray,0,(int)bytesread);
                        Marshal.ReleaseComObject(stream);
                        fstream.Close();
                        
                        stream = null;
                    }
                }
            }
            return res;
        }

        private PortableDeviceTypesLib._tagpropertykey GetPropertyKey(string identifier)
        {
            StreamReader rdr;
            string line;
            Guid gd;
            Match m=null;
            Regex rg=new Regex("DEFINE_PROPERTYKEY\\s*\\(\\s*" + identifier + "\\s*,\\s*(?<fmtidpre>0x[a-fA-F0-9]{8}\\s*,\\s*(0x[a-fA-F0-9]{4}\\s*,\\s*){2})(?<fmtidgrp>(0x[a-fA-F0-9]{2}\\s*,\\s*){7}0x[a-fA-F0-9]{2})\\s*,\\s*(?<pid>[0-9]+)");
            PortableDeviceTypesLib._tagpropertykey res = new PortableDeviceTypesLib._tagpropertykey();
            try
            {
                rdr = File.OpenText( System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".dll","") + "PortableDevice.h");
            }
            catch
            {
                throw new PTCAccessException("PortableDevice.h could not be found");
            }
            while ((line = rdr.ReadLine()) != null)
            {
                if ((m = rg.Match(line)).Success)
                {
                    break;
                }
            }
            if (m != null)
            {
                gd = new Guid("{" + m.Groups["fmtidpre"].Value + "{" + m.Groups["fmtidgrp"].Value + "}}");
                res.fmtid = gd;
                res.pid = Convert.ToUInt32(m.Groups["pid"].Value);
            }
            else
            {
                throw new PTCAccessException("the identifier \"" + identifier + "\" could not be found");
            }
            rdr.Close();
            return res;
        }

        private PortableDeviceApiLib._tagpropertykey GetApiPropertyKey(string identifier)
        {
            StreamReader rdr;
            string line;
            Guid gd;
            Match m = null;
            Regex rg = new Regex("DEFINE_PROPERTYKEY\\s*\\(\\s*" + identifier + "\\s*,\\s*(?<fmtidpre>0x[a-fA-F0-9]{8}\\s*,\\s*(0x[a-fA-F0-9]{4}\\s*,\\s*){2})(?<fmtidgrp>(0x[a-fA-F0-9]{2}\\s*,\\s*){7}0x[a-fA-F0-9]{2})\\s*,\\s*(?<pid>[0-9]+)");
            PortableDeviceApiLib._tagpropertykey res = new PortableDeviceApiLib._tagpropertykey();
            try
            {
                rdr = File.OpenText(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".dll", "") + "PortableDevice.h");
            }
            catch
            {
                throw new PTCAccessException("PortableDevice.h could not be found");
            }
            while ((line = rdr.ReadLine()) != null)
            {
                if ((m = rg.Match(line)).Success)
                {
                    break;
                }
            }
            if (m != null)
            {
                gd = new Guid("{" + m.Groups["fmtidpre"].Value + "{" + m.Groups["fmtidgrp"].Value + "}}");
                res.fmtid = gd;
                res.pid = Convert.ToUInt32(m.Groups["pid"].Value);
            }
            else
            {
                throw new PTCAccessException("the identifier \"" + identifier + "\" could not be found");
            }
            rdr.Close();
            return res;
        }

        private string GetContentType(Guid uid)
        {
            string res = null;
            StreamReader rdr;
            string line;
            Guid gd;
            Match m = null;
            Regex rg = new Regex("DEFINE_GUID\\s*\\(\\s*(?<cntdescr>[A-Za-z0-9_]+)\\s*,\\s*(?<fmtidpre>0x[a-fA-F0-9]{8}\\s*,\\s*(0x[a-fA-F0-9]{4}\\s*,\\s*){2})(?<fmtidgrp>(0x[a-fA-F0-9]{2}\\s*,\\s*){7}0x[a-fA-F0-9]{2})");
            try
            {
                rdr = File.OpenText(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".dll", "") + "PortableDevice.h");
            }
            catch
            {
                throw new PTCAccessException("PortableDevice.h could not be found");
            }
            while ((line = rdr.ReadLine()) != null)
            {
                if ((m = rg.Match(line)).Success)
                {
                    gd = new Guid("{" + m.Groups["fmtidpre"].Value + "{" + m.Groups["fmtidgrp"].Value + "}}");
                    if (gd == uid)
                    {
                        res = m.Groups["cntdescr"].Value;
                        break;
                    }
                }
            }
            rdr.Close();
            return res;
        }

        private Guid GetContentTypeGuid(string contentType)
        {
            StreamReader rdr;
            string line;
            Guid gd;
            Match m = null;
            Regex rg = new Regex("DEFINE_GUID\\s*\\(\\s*" + contentType + "\\s*,\\s*(?<fmtidpre>0x[a-fA-F0-9]{8}\\s*,\\s*(0x[a-fA-F0-9]{4}\\s*,\\s*){2})(?<fmtidgrp>(0x[a-fA-F0-9]{2}\\s*,\\s*){7}0x[a-fA-F0-9]{2})");
            try
            {
                string ptest = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".dll", "") + "PortableDevice.h";
                rdr = File.OpenText(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".dll", "") + "PortableDevice.h");
            }
            catch
            {
                throw new PTCAccessException("PortableDevice.h could not be found");
            }
            while ((line = rdr.ReadLine()) != null)
            {
                if ((m = rg.Match(line)).Success)
                {
                    break;
                }
            }
            if (m != null)
            {
                gd = new Guid("{" + m.Groups["fmtidpre"].Value + "{" + m.Groups["fmtidgrp"].Value + "}}");

            }
            else
            {
                throw new PTCAccessException("the identifier \"" + contentType + "\" could not be found");
            }
            rdr.Close();
            return gd;
        }
    }
}
