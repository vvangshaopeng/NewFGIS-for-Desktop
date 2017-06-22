using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helper
{
    public class PathHelper
    {
        public static string SystemBasePath
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static string ResourcePath
        {
            get
            {
                return SystemBasePath + "../Images/";
            }
        }
    }
}
