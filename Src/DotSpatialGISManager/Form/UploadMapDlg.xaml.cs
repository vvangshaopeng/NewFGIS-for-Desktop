using DotSpatial.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DotSpatial.Controls;
using DotSpatial.Data;
using Microsoft.Win32;

using System.Web;
using System.Net;
using System.IO;
using System.Drawing;
using System.Xml;

namespace DotSpatialGISManager
{
    /// <summary>
    /// ExportShpDlg.xaml 的交互逻辑
    /// </summary>
    public partial class UploadMapDlg : Window
    {
        private string xmlPath = Common.Helper.PathHelper.SystemBasePath + "Config/ServerConfig.xml";
        public UploadMapDlg()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();
            f.AddExtension = true;
            f.Filter = @"Map File(*.bmp;*.jpeg;*.jpg;*.tiff;*.png)|*.bmp;*.jpeg;*.jpg;*.tiff;*.png";
            f.Title = "Select map file";
            if (f.ShowDialog() == true)
            {
                //获取地图图片路径
                this.txtPath.Text = f.FileName;
            }
        }


        private void btnOK_Click(object sender, RoutedEventArgs e)   //上传地图图片
        {
            if (!System.IO.File.Exists(xmlPath))
            {
                ServerConfigDlg f = new ServerConfigDlg();
                if (f.ShowDialog() == false)
                    return;
            }

            //post函数参数
            string url = "";
            string name = "";
            string pwd = "";
            string label = this.txtName.Text;
            //string url = "http://139.129.166.245:8069/uploadMap";
            //string name = "name";
            //string pwd = "sddsdsf";

            try
            {
                //解析XML
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);
                //查找<Root>  
                XmlNode root = xmlDoc.SelectSingleNode("Root");
                //获取到所有<Root>的子节点  
                XmlNodeList nodeList = root.ChildNodes;
                //遍历所有子节点  
                foreach (XmlNode xn in nodeList)
                {
                    switch(xn.Name.ToLower())
                    {
                        case "url":
                            url = xn.InnerText;
                            break;
                        case "user":
                            name = xn.InnerText;
                            break;
                        case "password":
                            pwd = xn.InnerText;
                            break;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error");
                return;
            }

            if (url == ""||name=="")
            {
                MessageBox.Show("url is empty or name is empty");
                return;
            }

            string filePath = this.txtPath.Text;
            string fileName = System.IO.Path.GetFileName(filePath);

            //图名参数
            string mapName = this.txtName.Text;

            //转二进制
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] fileContentByte = new byte[fs.Length]; // 二进制文件
            fs.Read(fileContentByte, 0, Convert.ToInt32(fs.Length));
            fs.Close();

            #region 定义请求体中的内容 并转成二进制
            string boundary = "---011000010111000001101001";
            string Enter = "\r\n";
            //构建文件流字段
            //string modelIdStr = "--"+boundary+Enter+"Content-Disposition: form-data; name=\"modelId\""+Enter+Enter+modelId+Enter;
            string fileContentStr = "--" + boundary + Enter+ "Content-Type:application/octet-stream" + Enter + "Content-Disposition: form-data; name=\"img\"; filename=\"" + fileName + "\"" + Enter + Enter;
            string labelStr = Enter + "--" + boundary + Enter + "Content-Disposition: form-data; name=\"label\"" + Enter + Enter + label + Enter + "--" + boundary;
            //string updateTimeStr = Enter + "--" + boundary + Enter+ "Content-Disposition: form-data; name=\"updateTime\"" + Enter + Enter+ updateTime;
            //string encryptStr = Enter + "--" + boundary + Enter+ "Content-Disposition: form-data; name=\"encrypt\"" + Enter + Enter+ encrypt + Enter + "--" + boundary;
            string nameStr = Enter + "--" + boundary + Enter + "Content-Disposition: form-data; name=\"name\"" + Enter + Enter + name + Enter + "--" + boundary;
            string pwdStr = Enter + "--" + boundary + Enter + "Content-Disposition: form-data; name=\"pwd\"" + Enter + Enter + pwd + Enter + "--" + boundary + "--";

            //文件流转二进制
            //var modelIdStrByte = Encoding.UTF8.GetBytes(modelIdStr);//modelId所有字符串二进制
            var fileContentStrByte = Encoding.UTF8.GetBytes(fileContentStr);//fileContent一些名称等信息的二进制（不包含文件本身）
            var labelStrByte = Encoding.UTF8.GetBytes(labelStr);
            //var updateTimeStrByte = Encoding.UTF8.GetBytes(updateTimeStr);//updateTime所有字符串二进制
            //var encryptStrByte = Encoding.UTF8.GetBytes(encryptStr);//encrypt所有字符串二进制
            var nameStrByte = Encoding.UTF8.GetBytes(nameStr);
            var pwdStrByte = Encoding.UTF8.GetBytes(pwdStr); 


            #endregion

            //post
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "multipart/form-data;boundary=" + boundary;
            //定义请求流
            Stream myRequestStream = request.GetRequestStream();
            #region 将各个二进制 按顺序写入请求流 modelIdStr -> (fileContentStr + fileContent) -> uodateTimeStr -> encryptStr

            //myRequestStream.Write(modelIdStrByte, 0, modelIdStrByte.Length);
            myRequestStream.Write(fileContentStrByte, 0, fileContentStrByte.Length);
            myRequestStream.Write(fileContentByte, 0, fileContentByte.Length);
            //myRequestStream.Write(updateTimeStrByte, 0, updateTimeStrByte.Length);
            //myRequestStream.Write(encryptStrByte, 0, encryptStrByte.Length);
            myRequestStream.Write(nameStrByte, 0 , nameStrByte.Length);
            myRequestStream.Write(labelStrByte, 0, labelStrByte.Length);
            myRequestStream.Write(pwdStrByte, 0, pwdStrByte.Length);

            #endregion
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();//发送
            }
            catch(Exception ex)
            {
                MessageBox.Show("Uploading map failed");
                return;
            }

            Stream myResponseStream = response.GetResponseStream();//获取返回值
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

            string retString = myStreamReader.ReadToEnd();
            if (retString == "ok")
            {
                MessageBox.Show("Uploading map succeed!");
            }

            myStreamReader.Close();
            myResponseStream.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
