using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using PortableDeviceApiLib;

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

        public string[] GetDevices()
        { 
            string[] devices=null;
            uint ndevices=0;
            _manager.GetDevices(null,ref ndevices);
            if (ndevices > 0)
            {
                devices=new string[ndevices];
                _manager.GetDevices(devices, ref ndevices);
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

        public List<string> GetFolders(string parentFolder,string deviceID)
        {
            IPortableDevice dev;
            IPortableDeviceContent cnt;
            IEnumPortableDeviceObjectIDs oids;
            IPortableDeviceProperties props;
            IPortableDeviceValues propVals;
            Guid contentType;
            uint fetched=NUMBER_OBJECTS;
            string[] bfr;
            string folderName;
            List<string> res = new List<string>();
            dev = this.OpenDevice(deviceID);
            dev.Content(out cnt);
            if(parentFolder==null)
            {
                parentFolder="DEVICE";
            }
            cnt.EnumObjects(0, parentFolder, null, out oids);
            bfr = new string[NUMBER_OBJECTS];
            while (fetched == NUMBER_OBJECTS)
            {
                oids.Next(NUMBER_OBJECTS, bfr, ref fetched);
                for (int K = 0; K < fetched; K++)
                {
                    res.Add(bfr[K]);
                }
            }
            cnt.Properties(out props);
            foreach(string devID in res)
            {
                props.GetValues(devID, null, out propVals);
                propVals.GetStringValue(GetApiPropertyKey("WPD_OBJECT_NAME"), out folderName);
                propVals.GetGuidValue(GetApiPropertyKey("WPD_OBJECT_CONTENT_TYPE"), out contentType);
                string ctype = GetContentType(contentType);
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
                string ptest = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".dll","") + "PortableDevice.h";
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
                string tstr = "{" + m.Groups["fmtidpre"].Value + "{" + m.Groups["fmtidgrp"].Value + "}}";
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
                string tstr = "{" + m.Groups["fmtidpre"].Value + "{" + m.Groups["fmtidgrp"].Value + "}}";
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
    }
}
