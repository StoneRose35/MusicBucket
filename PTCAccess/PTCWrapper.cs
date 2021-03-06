﻿using System;
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
        private const uint NUMBER_OBJECTS = 10;

        // STGM constants
        const long STGM_READ = 0x00000000L;
        const long STGM_WRITE = 0x00000001L;
        const long STGM_READWRITE = 0x00000002L;
        const long STGM_SHARE_DENY_NONE = 0x00000040L;
        const long STGM_SHARE_DENY_READ = 0x00000030L;
 	    const long STGM_SHARE_DENY_WRITE = 0x00000020L;
 	    const long STGM_SHARE_EXCLUSIVE	= 0x00000010L;
 	    const long STGM_PRIORITY = 0x00040000L;
        const long STGM_CREATE = 0x00001000L;
 	    const long STGM_CONVERT = 0x00020000L;
 	    const long STGM_FAILIFTHERE = 0x00000000L;
        const long STGM_DIRECT = 0x00000000L;
 	    const long STGM_TRANSACTED = 0x00010000L;
        const long STGM_NOSCRATCH = 0x00100000L;
 	    const long STGM_NOSNAPSHOT = 0x00200000L;
        const long STGM_SIMPLE = 0x08000000L;
 	    const long STGM_DIRECT_SWMR = 0x00400000L;
        const long STGM_DELETEONRELEASE = 0x04000000L;

        public PTCWrapper()
        {
        }

        public static List<PTCDevice> GetDevices()
        { 
            List<PTCDevice> devices=new List<PTCDevice>();
            PortableDeviceManager mgr = new PortableDeviceManager();
            uint ndevices=0;
            string[] devicesStr;
            string friendlyName=null;
            uint nbytes=0;
            mgr.GetDevices(null,ref ndevices);
            if (ndevices > 0)
            {
                devicesStr = new string[ndevices];
                mgr.GetDevices(devicesStr, ref ndevices);
                foreach (string dev in devicesStr) // get clear text name of each device
                {
                    nbytes = 0;
                    mgr.GetDeviceFriendlyName(dev, null, ref nbytes);
                    friendlyName = new string((char)0, (int)(nbytes-1));
                    mgr.GetDeviceFriendlyName(dev, friendlyName, ref nbytes);
                    devices.Add(new PTCDevice() { ID = dev, Name = friendlyName });
                }
            }
            return devices;
        }

        public static IPortableDevice OpenDevice(string deviceID)
        {
            IPortableDevice res;
            PortableDeviceTypesLib._tagpropertykey test;
            PortableDeviceTypesLib.PortableDeviceValues vals;
            vals = new PortableDeviceTypesLib.PortableDeviceValues();
            try
            {
                res = new PortableDeviceFTM();
            }
            catch
            {
                res = new PortableDevice();
            }
            test = GetPropertyKey("WPD_CLIENT_NAME");
            vals.SetStringValue(GetPropertyKey("WPD_CLIENT_NAME"), "MusicBucket");
            vals.SetUnsignedIntegerValue(GetPropertyKey("WPD_CLIENT_MAJOR_VERSION"), 0);
            vals.SetUnsignedIntegerValue(GetPropertyKey("WPD_CLIENT_MINOR_VERSION"), 1);
            vals.SetUnsignedIntegerValue(GetPropertyKey("WPD_CLIENT_REVISION"), 42);
            res.Open(deviceID,(PortableDeviceApiLib.IPortableDeviceValues)vals);
            return res;
        }

        public static List<PTCFolder> GetFolders(string parentFolder,string deviceID)
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
            dev = OpenDevice(deviceID);
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
                        if(parentFolder=="DEVICE" && GetFolders(bfr[K],deviceID).Count>0)
                        {
                            res.Add(new PTCFolder() { Id = bfr[K], Name = fname, Uid = fuid,IsRootFolder=true,DeviceID=deviceID,Parent=parent });
                        }
                    }
                }
            }
            
            return res;
        }

        public static void GetMp3FileNames(PTCFolder parentFolder, ref List<PTCFile> filelist, bool scansubfolders)
        {
            GetMp3FileNames(parentFolder.Id, parentFolder.DeviceID, ref filelist, scansubfolders);
        }

        public static void GetMp3FileNames(string folderID, string deviceID, ref List<PTCFile> filelist,bool scansubfolder)
        {
            IPortableDevice dev;
            IPortableDeviceContent content;
            IPortableDeviceResources resources;
            IPortableDeviceProperties props;
            IPortableDeviceKeyCollection keyColl;
            IPortableDeviceValues vals;
            IEnumPortableDeviceObjectIDs oids;
            Guid contentType;
            string filename,fuid;
            string[] bfr;
            uint fetched = NUMBER_OBJECTS;
            if (filelist == null)
            {
                filelist = new List<PTCFile>();
            }
            keyColl=(new PortableDeviceTypesLib.PortableDeviceKeyCollection()) as IPortableDeviceKeyCollection;
            bfr = new string[NUMBER_OBJECTS];
            dev = OpenDevice(deviceID);
            dev.Content(out content);
            content.Properties(out props);
            keyColl.Add(GetApiPropertyKey("WPD_OBJECT_ORIGINAL_FILE_NAME"));
            keyColl.Add(GetApiPropertyKey("WPD_OBJECT_CONTENT_TYPE"));
            keyColl.Add(GetApiPropertyKey("WPD_OBJECT_NAME"));
            keyColl.Add(GetApiPropertyKey("WPD_OBJECT_PERSISTENT_UNIQUE_ID"));
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
                    vals.GetStringValue(GetApiPropertyKey("WPD_OBJECT_PERSISTENT_UNIQUE_ID"), out fuid);
                    if (contentType == GetContentTypeGuid("WPD_CONTENT_TYPE_AUDIO"))
                    {
                        if(filename.EndsWith(".mp3"))
                        {
                            filelist.Add(new PTCFile() { Name=filename,Id=bfr[K], DeviceID=deviceID, Uid=fuid});
                        }
                    }
                    if (scansubfolder)
                    {
                        if (contentType == GetContentTypeGuid("WPD_CONTENT_TYPE_FOLDER"))
                        {
                            GetMp3FileNames(bfr[K], deviceID,ref filelist, true);
                        }
                    }
                }
            }
        }

        public static void CopyMp3File(string pathToFile,string finalname,PTCFolder parentFolder)
        {
            uint optimalbuffersize=0;
            string cookie = "";
            FileStream fstream=null;
            IPortableDevice dev;
            IPortableDeviceContent cnt;
            int bytesread;
            IntPtr bwritten;
            uint byteswritten;
            byte[] bytearray;
            PortableDeviceTypesLib.IPortableDeviceValues vals;
            Guid contentType;
            IStream res = null;
            dev = OpenDevice(parentFolder.DeviceID);
            dev.Content(out cnt);
            contentType = GetContentTypeGuid("WPD_CONTENT_TYPE_AUDIO");
            vals = new PortableDeviceTypesLib.PortableDeviceValues();

            try 
            {
                fstream = new FileStream(pathToFile, FileMode.Open);
                // set required values
                vals.SetStringValue(GetPropertyKey("WPD_OBJECT_PARENT_ID"), parentFolder.Id);
                vals.SetStringValue(GetPropertyKey("WPD_OBJECT_NAME"), finalname);
                vals.SetStringValue(GetPropertyKey("WPD_OBJECT_ORIGINAL_FILE_NAME"), finalname.Replace("?",""));
                vals.SetGuidValue(GetPropertyKey("WPD_OBJECT_FORMAT"), GetContentTypeGuid("WPD_OBJECT_FORMAT_MP3"));
                vals.SetGuidValue(GetPropertyKey("WPD_OBJECT_CONTENT_TYPE"), GetContentTypeGuid("WPD_CONTENT_TYPE_AUDIO"));
                vals.SetBoolValue(GetPropertyKey("WPD_OBJECT_CAN_DELETE"), 1);
                vals.SetUnsignedLargeIntegerValue(GetPropertyKey("WPD_OBJECT_SIZE"), (ulong)fstream.Length);
                cnt.CreateObjectWithPropertiesAndData((IPortableDeviceValues)vals, out res, ref optimalbuffersize, ref cookie);
                bytesread = (int)optimalbuffersize;
                bytearray = new byte[optimalbuffersize];
                bwritten = Marshal.AllocHGlobal(sizeof(ulong));
                bytesread = fstream.Read(bytearray, 0, (int)optimalbuffersize);
                while (bytesread == optimalbuffersize)
                {
                    ((System.Runtime.InteropServices.ComTypes.IStream)res).Write(bytearray, bytesread, bwritten);
                    byteswritten = (uint)Marshal.ReadInt64(bwritten);
                    if (bytesread != byteswritten)
                    {
                        throw new PTCAccessException("error writing to mobile device buffer");
                    }
                    bytesread = fstream.Read(bytearray, 0, (int)bytesread);
                }
                ((System.Runtime.InteropServices.ComTypes.IStream)res).Write(bytearray, (int)bytesread, bwritten);
                byteswritten = (uint)Marshal.ReadInt64(bwritten);
                if (bytesread != byteswritten)
                {
                    throw new PTCAccessException("error writing to mobile device buffer");
                }
                res.Commit(0);
            }
            catch (Exception exc)
            {
                res.Revert();
                throw new PTCAccessException("Error during copying file to mobile device",exc);
            }
            finally
            {
                Marshal.ReleaseComObject(res);
                if(fstream!=null)
                {
                    fstream.Close();
                }   
            }
            

        }

        /// <summary>
        /// Gets a memory stream containing the mp3 file
        /// </summary>
        /// <param name="mp3file">The file to read</param>
        /// <returns>the memory stream holding the entire mp3 file</returns>
        public static Stream GetMp3Stream(PTCFile mp3file)
        {
            Stream res = null;
            IPortableDevice dev;
            IPortableDeviceContent content;
            IPortableDeviceResources resources;
            IPortableDeviceProperties props;
            IPortableDevicePropVariantCollection variantColl,variantCollOut;
            tag_inner_PROPVARIANT propvariant,propvarout;
            IntPtr bread,uidfield,uidfieldout;
            IStream stream;
            uint optimalbuffersize = 0;
            byte[] bytearray;
            uint bytesread;
            byte[] barray;
            dev = OpenDevice(mp3file.DeviceID);
            dev.Content(out content);
            content.Properties(out props);
            content.Transfer(out resources);

            variantColl=(IPortableDevicePropVariantCollection)(new PortableDeviceTypesLib.PortableDevicePropVariantCollection());
            propvariant=new tag_inner_PROPVARIANT();
            propvariant.vt=31;
            barray=UnicodeEncoding.Unicode.GetBytes(mp3file.Uid);
            barray = barray.Concat(new byte[]{0,0}).ToArray();
            uidfield = Marshal.AllocHGlobal(barray.Length);
            Marshal.Copy(barray, 0, uidfield, barray.Length);
            propvariant.wVal = uidfield.ToInt32();
            variantColl.Add(propvariant);
            content.GetObjectIDsFromPersistentUniqueIDs(variantColl, out variantCollOut);
            propvarout = new tag_inner_PROPVARIANT();
            variantCollOut.GetAt(0, ref propvarout);
            uidfieldout = new IntPtr(propvarout.wVal);
            mp3file.Id = Marshal.PtrToStringUni(uidfieldout);
            Marshal.FreeHGlobal(uidfield);
            Marshal.FreeHGlobal(uidfieldout);

            resources.GetStream(mp3file.Id, GetApiPropertyKey("WPD_RESOURCE_DEFAULT"), (uint)STGM_READ, ref optimalbuffersize, out stream);
            bytearray = new byte[optimalbuffersize];
            bytesread = optimalbuffersize;
            res = new MemoryStream(); 
            bread = Marshal.AllocHGlobal(sizeof(ulong));
            while (bytesread == optimalbuffersize)
            {
                ((System.Runtime.InteropServices.ComTypes.IStream)stream).Read(bytearray, (int)optimalbuffersize, bread);
                bytesread = (uint)Marshal.ReadInt64(bread);
                res.Write(bytearray, 0, (int)bytesread);
            }
            Marshal.FreeHGlobal(bread);
            res.Write(bytearray, 0, (int)bytesread);
            Marshal.ReleaseComObject(stream);
            return res;
        }

        /// <summary>
        /// Copies the mp3 file from the mobile device to the file system
        /// </summary>
        /// <param name="mp3file">the file to copy</param>
        /// <param name="outpath">the copy destination, must not exist already!</param>
        public static void CopyFromMobileDevice(PTCFile mp3file, string outpath)
        {
            Stream res = null;
            IPortableDevice dev;
            IPortableDeviceContent content;
            IPortableDeviceResources resources;
            IPortableDeviceProperties props;
            IPortableDevicePropVariantCollection variantColl, variantCollOut;
            tag_inner_PROPVARIANT propvariant, propvarout;
            IntPtr bread, uidfield, uidfieldout;
            IStream stream;
            uint optimalbuffersize = 0;
            byte[] bytearray;
            uint bytesread;
            byte[] barray;
            dev = OpenDevice(mp3file.DeviceID);
            dev.Content(out content);
            content.Properties(out props);
            content.Transfer(out resources);

            variantColl = (IPortableDevicePropVariantCollection)(new PortableDeviceTypesLib.PortableDevicePropVariantCollection());
            propvariant = new tag_inner_PROPVARIANT();
            propvariant.vt = 31;
            barray = UnicodeEncoding.Unicode.GetBytes(mp3file.Uid);
            barray = barray.Concat(new byte[] { 0, 0 }).ToArray();
            uidfield = Marshal.AllocHGlobal(barray.Length);
            Marshal.Copy(barray, 0, uidfield, barray.Length);
            propvariant.wVal = uidfield.ToInt32();
            variantColl.Add(propvariant);
            content.GetObjectIDsFromPersistentUniqueIDs(variantColl, out variantCollOut);
            propvarout = new tag_inner_PROPVARIANT();
            variantCollOut.GetAt(0, ref propvarout);
            uidfieldout = new IntPtr(propvarout.wVal);
            mp3file.Id = Marshal.PtrToStringUni(uidfieldout);
            Marshal.FreeHGlobal(uidfield);
            Marshal.FreeHGlobal(uidfieldout);

            resources.GetStream(mp3file.Id, GetApiPropertyKey("WPD_RESOURCE_DEFAULT"), (uint)STGM_READ, ref optimalbuffersize, out stream);
            bytearray = new byte[optimalbuffersize];
            bytesread = optimalbuffersize;
            res = new FileStream(outpath, FileMode.CreateNew);
            bread = Marshal.AllocHGlobal(sizeof(ulong));
            while (bytesread == optimalbuffersize)
            {
                ((System.Runtime.InteropServices.ComTypes.IStream)stream).Read(bytearray, (int)optimalbuffersize, bread);
                bytesread = (uint)Marshal.ReadInt64(bread);
                res.Write(bytearray, 0, (int)bytesread);
            }
            Marshal.FreeHGlobal(bread);
            res.Write(bytearray, 0, (int)bytesread);
            res.Close();
            Marshal.ReleaseComObject(stream);
        }

        /// <summary>
        /// Checks of a path on a PTC device exists The path has the following structure:
        /// [deviceFriendyName]:\Folder1\SubFolder1\SubFolder2 ...
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true of the path exists, false otherwise</returns>
        public static bool Exists(string path,out PTCFolder lastFolder)
        {
            bool res = true;
            string dev;
            string[] folders;
            string folderID=null;
            lastFolder = null;
            dev = Regex.Match(path, "\\[([a-zA-Z0-9-_\\+\\*%&/\\(\\)~]+)\\]").Groups[1].Value;
            PTCDevice dev2;
            try
            {
                dev2 = GetDevices().Single(s => s.Name == dev);
            }
            catch
            {
                dev2 = null;
            }
            if (dev2 != null) // found a device with the same name
            {
                path = path.Replace(dev2.ToString(), "").TrimEnd('\\');
                folders = path.Split('\\');
                for (int cnt = 0; cnt < folders.Length; cnt++)
                {
                    if (cnt == 0)
                    {
                        PTCFolder fld = GetFolders(null, dev2.ID).Single(s => s.Equals(new PTCFolder() { Name = folders[cnt], DeviceID = dev2.ID }));
                        if (fld != null)
                        {
                            folderID = fld.Id;
                            lastFolder = fld;
                        }
                        else
                        {
                            res = false;
                            break;
                        }
                    }
                    else
                    {
                        PTCFolder fld = GetFolders(folderID, dev2.ID).Single(s => s.Equals(new PTCFolder() { Name = folders[cnt], DeviceID = dev2.ID }));
                        if (fld != null)
                        {
                            folderID = fld.Id;
                            lastFolder = fld;
                        }
                        else
                        {
                            res = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                res = false;
            }
            return res;
        }

        private static PortableDeviceTypesLib._tagpropertykey GetPropertyKey(string identifier)
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

        private static PortableDeviceApiLib._tagpropertykey GetApiPropertyKey(string identifier)
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

        private static string GetContentType(Guid uid)
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

        private static Guid GetContentTypeGuid(string contentType)
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
