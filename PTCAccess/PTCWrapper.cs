﻿using System;
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
        private string _currentDevice;
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

        public List<PTCFolder> GetFolders(string parentFolder,string deviceID)
        {
            IPortableDevice dev;
            IPortableDeviceContent cnt;
            IEnumPortableDeviceObjectIDs oids;
            IPortableDeviceProperties props;
            IPortableDeviceValues propVals;
            IPortableDeviceKeyCollection keyColl;
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
                        res.Add(new PTCFolder() { Id=bfr[K],Name=fname,Uid=fuid});
                    }
                    else if (contentType == GetContentTypeGuid("WPD_CONTENT_TYPE_FUNCTIONAL_OBJECT"))
                    {
                        if(GetFolders(bfr[K],deviceID).Count>0)
                        {
                            res.Add(new PTCFolder() { Id = bfr[K], Name = fname, Uid = fuid });
                        }
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
