using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTCAccess
{
    public class PTCAccessException : Exception
    {
        public PTCAccessException(string message)
            : base(message)
        { }
    }
}
