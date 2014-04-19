using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDDBAccess
{
    /// <summary>
    /// Thrown when one tries a save a illegal disc id, a legal discid contains 8 characters
    /// containing letters (a-z or A-Z) and digits only
    /// </summary>
    public class IllegalDiscIDException : Exception
    {
    }
}
