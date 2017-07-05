using DotSpatial.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace DotSpatialGISManager
{
    /// <summary>
    /// SetScaleDlg.xaml 的交互逻辑
    /// </summary>
    public partial class SetScaleDlg : Window
    {
        private double m_CurrentScale;
        public SetScaleDlg()
        {
            InitializeComponent();
            this.cboScale.Items.Add("10,000");
            this.cboScale.Items.Add("50,000");
            this.cboScale.Items.Add("100,000");
            this.cboScale.Items.Add("500,000");
            this.cboScale.Items.Add("1,000,000");
            this.cboScale.Items.Add("5,000,000");
            this.cboScale.Items.Add("10,000,000");
        }

        public void Init()
        {
            this.cboScale.Text = ComputeMapScale();
            double.TryParse(this.cboScale.Text, out m_CurrentScale);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.m_SetScaleDlg = null;
        }

        /// <summary>
        /// 计算两个点的距离  （米）
        /// </summary>
        /// <param name="dDegX1"></param>
        /// <param name="dDegY1"></param>
        /// <param name="dDegX2"></param>
        /// <param name="dDegY2"></param>
        /// <returns></returns>
        private static double MetersFromDecimalDegreesPoints(double dDegX1, double dDegY1, double dDegX2, double dDegY2)
        {
            try
            {
                const double DRadius = 6378007; // radius of Earth in meters
                const double DCircumference = DRadius * 2 * Math.PI;
                const double DMetersPerLatDd = 111113.519;

                double dDeltaXdd = Math.Abs(dDegX1 - dDegX2);
                double dDeltaYdd = Math.Abs(dDegY1 - dDegY2);
                double dCenterY = (dDegY1 + dDegY2) / 2.0;
                double dMetersPerLongDd = (Math.Cos(dCenterY * (Math.PI / 180.0)) * DCircumference) / 360.0;
                double dDeltaXmeters = dMetersPerLongDd * dDeltaXdd;
                double dDeltaYmeters = DMetersPerLatDd * dDeltaYdd;

                return Math.Sqrt(Math.Pow(dDeltaXmeters, 2.0) + Math.Pow(dDeltaYmeters, 2.0));
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// 获取当前视图比例尺
        /// </summary>
        private string ComputeMapScale()
        {
            try
            {
                const double DInchesPerMeter = 39.3700787401575;
                const double DDegreesPerRadian = 57.2957;
                double dMapWidthInMeters;

                if (MainWindow.m_DotMap.Projection == null)
                    return ""; 

                if (MainWindow.m_DotMap.Projection.IsLatLon)
                {
                    var dMapWidthInRadians = MainWindow.m_DotMap.ViewExtents.Width * MainWindow.m_DotMap.Projection.GeographicInfo.Unit.Radians;
                    var dMapWidthInDegrees = dMapWidthInRadians * DDegreesPerRadian;
                    var dMapLatInRadians = MainWindow.m_DotMap.ViewExtents.Center.Y * MainWindow.m_DotMap.Projection.GeographicInfo.Unit.Radians;
                    var dMapLatInDegrees = dMapLatInRadians * DDegreesPerRadian;
                    dMapWidthInMeters = MetersFromDecimalDegreesPoints(0.0, dMapLatInDegrees, dMapWidthInDegrees, dMapLatInDegrees);
                }
                else
                {
                    dMapWidthInMeters = MainWindow.m_DotMap.ViewExtents.Width * MainWindow.m_DotMap.Projection.Unit.Meters;
                }

                // Get the number of pixels in one screen inch.
                // get resolution, most screens are 96 dpi, but you never know...
                double dScreenWidthInMeters = (Convert.ToDouble(MainWindow.m_DotMap.BufferedImage.Width) / MainWindow.m_DotMap.BufferedImage.HorizontalResolution) / DInchesPerMeter;
                double dMetersPerScreenMeter = dMapWidthInMeters / dScreenWidthInMeters;
                string res = dMetersPerScreenMeter.ToString("n0", CultureInfo.CurrentCulture);
                return res;
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 设置比例尺
        /// </summary>
        /// <param name="scale"></param>
        private void ScaleTo(double scale)
        {
            if (scale == 0 || MainWindow.m_DotMap.Projection == null) return;

            var ext = MainWindow.m_DotMap.ViewExtents;

            if (ext.Width != 0)
            {
                Point centerpoint = new Point((ext.MinX + ext.MaxX) / 2, (ext.MinY + ext.MaxY) / 2);
                const double DInchesPerMeter = 39.3700787401575;
                double dScreenWidthInMeters = (MainWindow.m_DotMap.BufferedImage.Width / MainWindow.m_DotMap.BufferedImage.HorizontalResolution) / DInchesPerMeter;
                //经纬度
                if (MainWindow.m_DotMap.Projection.IsLatLon)
                {
                    double width = ext.MaxX - ext.MinX;
                    double height = ext.MaxY - ext.MinY;
                    double pe = (double)(scale * 1.0 / m_CurrentScale);
                    MainWindow.m_DotMap.ViewExtents = new Extent((double)(centerpoint.X - 0.5 * width * pe), (double)(centerpoint.Y - 0.5 * height * pe), (double)(centerpoint.X + 0.5 * width * pe), (double)(centerpoint.Y + 0.5 * height * pe));
                }
                else
                {
                    double newwidth = ((scale * dScreenWidthInMeters) / MainWindow.m_DotMap.Projection.Unit.Meters) / 2;
                    double newheight = ((MainWindow.m_DotMap.ViewExtents.Height * newwidth) / MainWindow.m_DotMap.ViewExtents.Width) / 2;
                    MainWindow.m_DotMap.ViewExtents = new Extent(centerpoint.X - newwidth, centerpoint.Y - newheight, centerpoint.X + newwidth, centerpoint.Y + newheight);
                }   
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            double scale;
            if (double.TryParse(this.cboScale.Text,out scale))
            {
                ScaleTo(scale);
            }
            else
            {
                MessageBox.Show("Please input a number");
                return;
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            double scale;
            if (double.TryParse(this.cboScale.Text, out scale))
            {
                ScaleTo(scale);
                m_CurrentScale = scale;
            }
            else
            {
                MessageBox.Show("Please input a number");
                return;
            }
        }
    }
}
