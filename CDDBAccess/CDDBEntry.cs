using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CDDBAccess
{
    /// <summary>
    /// 
    /// Data object for a CDDB entry, contains only the identifying properties.
    /// </summary>
    public class CDDBEntry
    {
        private string _category;
        private string _discid;
        private string _content;
        private static List<string> _categorylist;

        public static List<string> Categories
        {
            get
            {
                if (_categorylist == null)
                {
                    string cats;
                    string[] splitted;
                    CDDBConnection conn = new CDDBConnection();
                    cats = conn.SendCustomCommand("cddb lscat");
                    splitted = cats.Split('\n');
                    _categorylist = new List<string>();
                    for (int u = 1; u < splitted.Length - 1; u++)
                    {
                        _categorylist.Add(splitted[u].TrimEnd('\r'));
                    }
                }
                return _categorylist;
            }
        }
        public string Category
        {
            get { return _category; }
            set
            {
                if (Categories.Contains(value))
                {
                    _category = value;
                }
                else
                {
                    throw new IllegalCategoryException();
                }
            }
        }

        public string DiscID
        {
            get { return _discid; }
            set 
            {
                if (Regex.Match(value, "[A-Za-z0-9]{8}").Success)
                {
                    _discid = value;
                }
                else
                {
                    throw new IllegalDiscIDException();
                }
            }
        }

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }
    }
}
