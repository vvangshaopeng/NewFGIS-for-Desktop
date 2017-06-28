using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// FieldCalculatorDlg.xaml 的交互逻辑
    /// </summary>
    public partial class FieldCalculatorDlg : Window
    {
        private List<IMapFeatureLayer> m_LayerList = null;
        private IFeatureSet m_CurrentFeaSet;
        private List<DataColumn> m_FieldList = null;
        //需要进行计算的字段
        private DataColumn m_CurrentCalField;
        //选择的参与计算的字段
        private DataColumn m_CurrentField;
        //计算后的字段
        private DataColumn m_ResultField;
        public FieldCalculatorDlg()
        {
            InitializeComponent();
            m_LayerList = MainWindow.m_DotMap.GetFeatureLayers().ToList();
            m_CurrentFeaSet = (m_LayerList[0] as FeatureLayer).FeatureSet;

            this.lstFields.Items.Clear();
            foreach (DataColumn col in m_CurrentFeaSet.DataTable.Columns)
            {
                try
                {
                    this.cboCalFields.Items.Add(col.ColumnName);
                    this.lstFields.Items.Add(col.ColumnName);
                    m_FieldList.Add(col);
                }
                catch
                {
                    Console.WriteLine("The current layer have no field, you should change a layer.");
                }
            }
            if (this.cboCalFields.Items.Count > 0)
            {
                this.cboCalFields.SelectedIndex = 0;
            }
        }

        private void cboCalFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboCalFields.SelectedIndex == -1) return;
            try
            {
                m_CurrentCalField = m_FieldList[this.cboCalFields.SelectedIndex];
            }
            catch
            {
                Console.WriteLine("The current layer have no field.");
            }
        }

        private void btnOpe_Click(object sender, RoutedEventArgs e)
        {
            string pBtnContent = (sender as Button).Content.ToString();
            this.txtExpression.Text += pBtnContent + " ";
        }

        private void txtExpression_TextChanged(object sender, TextChangedEventArgs e)
        {
            //如果txtExpression中未有任何字符，包括没有空格，则apply和清除按钮为灰色，模仿ArcGIS
            if (string.IsNullOrEmpty(this.txtExpression.Text))
            {
                this.btnOK.IsEnabled = false;
                this.btnClear.IsEnabled = false;
            }
            else if (string.IsNullOrWhiteSpace(this.txtExpression.Text))//仅仅包含空格，apply不可用，清空可用
            {
                this.btnOK.IsEnabled = false;
                this.btnClear.IsEnabled = true;
            }
            else
            {
                this.btnOK.IsEnabled = true;
                this.btnClear.IsEnabled = true;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (m_ResultField != null)
            {
                m_CurrentCalField = m_ResultField;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.txtExpression.Clear();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lstFields_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.lstFields.SelectedItem == null) return;
            this.txtExpression.Text += ("[" + this.lstFields.SelectedItem.ToString() + "] ");
            this.btnClear.IsEnabled = true;
        }

        private void lstFunctions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.lstFunctions.SelectedItem == null) return;
            this.txtExpression.Text += (this.lstFunctions.SelectedItem.ToString());
            this.btnClear.IsEnabled = true;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            OpenAttributeTableDlg.m_FieldCalculatorDlg = null;
        }
    }
}
