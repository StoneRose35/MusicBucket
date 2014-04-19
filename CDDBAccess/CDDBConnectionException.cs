using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDDBAccess
{
    public class CDDBConnectionException : Exception
    {
        public override string Message
        {
            get
            {
                return Properties.Resources.CDDBConnectionExceptionMessage;
            }
        }
    }
}
