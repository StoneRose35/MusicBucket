using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDDBAccess
{
    /// <summary>
    /// Thrown when one tries to set a category which is not present in the freedb category list
    /// </summary>
    public class IllegalCategoryException : Exception
    {
    }
}
