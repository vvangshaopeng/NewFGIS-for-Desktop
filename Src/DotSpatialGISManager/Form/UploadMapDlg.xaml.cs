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

namespace DotSpatialGISManager
{
    /// <summary>
    /// ExportShpDlg.xaml 的交互逻辑
    /// </summary>
    public partial class UploadMapDlg : Window
    {

        public UploadMapDlg()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();
            f.AddExtension = true;
            f.Filter = @"Map File(*.bmp;*.jpeg;*.jpg;*.tiff)|*.bmp;*.jpeg;*.jpg;*.tiff";
            f.Title = "Select map file";
            if (f.ShowDialog() == true)
            {
                //获取地图图片路径
                this.txtPath.Text = f.FileName;
            }
        }


        private void btnOK_Click(object sender, RoutedEventArgs e)   //上传地图图片
        {
            //post函数参数
            string url = "http://139.129.166.245:8069/uploadMap";
            string modelId = "cbb36ea1-68d6-2429-d37e-52b738240042";
            string updateTime = DateTime.Now.ToLocalTime().ToString();
            string encrypt = "sdsds";

            string filePath = this.txtPath.Text;
            string fileName = System.IO.Path.GetFileName(filePath);

            //转二进制
            byte[] fileContentByte = new byte[1024];
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            fileContentByte = new byte[fs.Length]; // 二进制文件
            fs.Read(fileContentByte, 0, Convert.ToInt32(fs.Length));
            fs.Close();

            #region 定义请求体中的内容 并转成二进制
            string boundary = "---011000010111000001101001";
            string Enter = "\r\n";
            //构建文件流字段
            string modelIdStr = "--"+boundary+Enter+"Content-Disposition: form-data; name=\"modelId\""+Enter+Enter+modelId+Enter;
            string fileContentStr = "--" + boundary + Enter+ "Content-Type:application/octet-stream" + Enter + "Content-Disposition: form-data; name=\"fileContent\"; filename=\"" + fileName + "\"" + Enter + Enter;
            string updateTimeStr = Enter + "--" + boundary + Enter+ "Content-Disposition: form-data; name=\"updateTime\"" + Enter + Enter+ updateTime;
            string encryptStr = Enter + "--" + boundary + Enter+ "Content-Disposition: form-data; name=\"encrypt\"" + Enter + Enter+ encrypt + Enter + "--" + boundary + "--";

            //文件流转二进制
            var modelIdStrByte = Encoding.UTF8.GetBytes(modelIdStr);//modelId所有字符串二进制
            var fileContentStrByte = Encoding.UTF8.GetBytes(fileContentStr);//fileContent一些名称等信息的二进制（不包含文件本身）
            var updateTimeStrByte = Encoding.UTF8.GetBytes(updateTimeStr);//updateTime所有字符串二进制
            var encryptStrByte = Encoding.UTF8.GetBytes(encryptStr);//encrypt所有字符串二进制


            #endregion

            //post
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "multipart/form-data;boundary=" + boundary;
            //定义请求流
            Stream myRequestStream = request.GetRequestStream();
            #region 将各个二进制 按顺序写入请求流 modelIdStr -> (fileContentStr + fileContent) -> uodateTimeStr -> encryptStr

            myRequestStream.Write(modelIdStrByte, 0, modelIdStrByte.Length);
            myRequestStream.Write(fileContentStrByte, 0, fileContentStrByte.Length);
            myRequestStream.Write(fileContentByte, 0, fileContentByte.Length);
            myRequestStream.Write(updateTimeStrByte, 0, updateTimeStrByte.Length);
            myRequestStream.Write(encryptStrByte, 0, encryptStrByte.Length);

            #endregion

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();//发送

            Stream myResponseStream = response.GetResponseStream();//获取返回值
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

            string retString = myStreamReader.ReadToEnd();


            myStreamReader.Close();
            myResponseStream.Close();

            //if (retString == "OK")
            //{
            //    MessageBox.Show("Uploading map succeeded.");
            //}
            //else
            //{
            //    MessageBox.Show("Uploading map failed.");
            //}
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
