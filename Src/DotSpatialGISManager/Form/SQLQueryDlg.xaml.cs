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
    /// SQLQueryDlg.xaml 的交互逻辑
    /// </summary>
    public partial class SQLQueryDlg : Window
    {
        private List<IMapFeatureLayer> m_LayerList = null;
        private IFeatureSet m_CurrentFeaSet = null;
        private bool m_IsRight = false;//sql的验证是否正确
        private List<IFeature> m_ResultFeatures = null;
        public SQLQueryDlg()
        {
            InitializeComponent();
            m_LayerList = MainWindow.m_DotMap.GetFeatureLayers().ToList();
            foreach (var layer in m_LayerList)
            {
                this.cboLyrs.Items.Add(layer.LegendText);
            }
            if (this.cboLyrs.Items.Count > 0)
            {
                this.cboLyrs.SelectedIndex = 0;
            }
        }

        private void cboLyrs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboLyrs.SelectedIndex == -1) return;
            m_CurrentFeaSet = (m_LayerList[this.cboLyrs.SelectedIndex] as FeatureLayer).FeatureSet;
            this.lstFields.Items.Clear();
            foreach (DataColumn col in m_CurrentFeaSet.DataTable.Columns)
            {
                this.lstFields.Items.Add(col.ColumnName);
            }
        }

        private void lstFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.lstUniqueVal.Items.Clear();
        }

        private void lst_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.lstFields.SelectedItem == null||this.lstUniqueVal.SelectedItem == null) return;
            if (m_CurrentFeaSet == null) return;
            string fieldname = this.lstFields.SelectedItem.ToString();
            DataColumn col = m_CurrentFeaSet.DataTable.Columns[fieldname];
            if (col.DataType == typeof(int)|| col.DataType == typeof(double)||col.DataType == typeof(long))
            {
                txtWhereClause.Text += this.lstUniqueVal.SelectedItem.ToString()+" ";
            }
            else
            {
                txtWhereClause.Text += "'" + this.lstUniqueVal.SelectedItem.ToString() + "' ";
            }
        }

        private void btnOpe_Click(object sender, RoutedEventArgs e)
        {
            string pBtnContent = (sender as Button).Content.ToString();
            this.txtWhereClause.Text += pBtnContent + " ";
        }

        private void btnGetUnique_Click(object sender, RoutedEventArgs e)
        {
            if (m_CurrentFeaSet == null) return;
            if (this.lstFields.SelectedItem == null) return;
            this.lstUniqueVal.Items.Clear();
            string fieldname = this.lstFields.SelectedItem.ToString();
            foreach (DataRow row in m_CurrentFeaSet.DataTable.Rows)
            {
                if (!this.lstUniqueVal.Items.Contains(row[fieldname]))
                {
                    this.lstUniqueVal.Items.Add(row[fieldname]);
                }
            }
        }

        private void txtWhereClause_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.m_IsRight = false;
            //如果txtWhereClause中未有任何字符，包括没有空格，则清除和检查按钮全部为灰色，模仿ArcGIS
            if (string.IsNullOrEmpty(this.txtWhereClause.Text))
            {
                this.btnOK.IsEnabled = false;
                this.btnClear.IsEnabled = false;
                this.btnCheck.IsEnabled = false;
            }
            else if (string.IsNullOrWhiteSpace(this.txtWhereClause.Text))//仅仅包含空格，清空可用，检查不可用
            {
                this.btnOK.IsEnabled = false;
                this.btnClear.IsEnabled = true;
                this.btnCheck.IsEnabled = false;
            }
            else
            {
                this.btnOK.IsEnabled = true;
                this.btnClear.IsEnabled = true;
                this.btnCheck.IsEnabled = true;
            }
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            if (this.m_IsRight)
            {
                MessageBox.Show("correct!");
            }
            else if (this.checkSql())
            {
                MessageBox.Show("correct!");
            }
            else
            {
                MessageBox.Show("error!");
            }
        }

        private bool checkSql()
        {
            if (m_CurrentFeaSet == null) return false;
            try
            {
                m_ResultFeatures = m_CurrentFeaSet.SelectByAttribute(this.txtWhereClause.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!checkSql())
            {
                MessageBox.Show("error!");
                return;
            }
            else
            {
                if (m_ResultFeatures != null)
                {
                    IFeatureLayer pLayer = m_LayerList[this.cboLyrs.SelectedIndex];
                    pLayer.UnSelectAll();
                    foreach (var fea in m_ResultFeatures)
                    {
                        pLayer.Select(fea);
                    }
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.txtWhereClause.Clear();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lstFields_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.lstFields.SelectedItem == null) return;
            this.txtWhereClause.Text += ("[" + this.lstFields.SelectedItem.ToString() + "] ");
            this.btnCheck.IsEnabled = true;
            this.btnClear.IsEnabled = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UCControls.UCQuery.m_SQLQueryDlg = null;
        }
    }
}
