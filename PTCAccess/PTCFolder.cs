using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PTCAccess
{
    public class PTCFolder
    {
        private string _name,_uid,_id;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string Uid
        {
            get
            {
                return _uid;
            }
            set
            {
                _uid = value;
            }
        }

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        public override string ToString()
        {
            return _name;
        }
    }
}
