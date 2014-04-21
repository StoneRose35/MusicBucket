using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTCAccess
{
    /// <summary>
    /// Class holding the most important information on a PTC device
    /// </summary>
    public class PTCDevice
    {
        private string _name, _id;

        /// <summary>
        /// The clear-text name
        /// </summary>
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


        /// <summary>
        /// The ID, used for referencing
        /// </summary>
        public string ID
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
