using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace DotSpatialGISManager
{
    /// <summary>
    /// ServerConfigDlg.xaml 的交互逻辑
    /// </summary>
    public partial class ServerConfigDlg : Window
    {
        private string ServerPath = Common.Helper.PathHelper.SystemBasePath + "Config/ServerConfig.xml";
        public ServerConfigDlg()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtUrl.Text.Trim() == ""|| this.txtUser.Text.Trim() == "")
            {
                MessageBox.Show("Please input Url or User");
                return;
            }
            if (!System.IO.Directory.Exists(Common.Helper.PathHelper.SystemBasePath + "Config/"));
                System.IO.Directory.CreateDirectory(Common.Helper.PathHelper.SystemBasePath + "Config/");
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(dec);
            //创建一个根节点（一级）
            XmlElement root = doc.CreateElement("Root");
            doc.AppendChild(root);
            //创建节点（二级）
            XmlNode node = doc.CreateElement("Url");
            node.InnerText = this.txtUrl.Text.Trim();
            root.AppendChild(node);
            node = doc.CreateElement("User");
            node.InnerText = this.txtUser.Text.Trim();
            root.AppendChild(node);
            node = doc.CreateElement("Password");
            node.InnerText = this.txtPassword.Password;
            root.AppendChild(node);
            doc.Save(ServerPath);
            this.DialogResult = true;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
