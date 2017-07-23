using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotSpatialGISManager
{
    public class HostConfig
    {
        private static string ServerPath = Common.Helper.PathHelper.SystemBasePath + "Config/ServerConfig.xml";
        public static void InitHostConfig()
        {
            if (!System.IO.File.Exists(ServerPath))
            {
                ServerConfigDlg f = new ServerConfigDlg();
                f.ShowDialog();
            }
        }
    }
}
