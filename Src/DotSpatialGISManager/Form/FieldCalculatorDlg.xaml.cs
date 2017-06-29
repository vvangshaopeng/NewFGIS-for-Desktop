using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Collections.ObjectModel;
using DotSpatialGISManager.Enum;
using Common;

namespace DotSpatialGISManager
{
    /// <summary>
    /// FieldCalculatorDlg.xaml 的交互逻辑
    /// </summary>
    public partial class FieldCalculatorDlg : Window, INotifyPropertyChanged
    {
        private IFeatureSet m_CurrentFeaSet;
        private FieldType m_FieldType;
        private string m_NewFieldName = null;

        #region 绑定
        private ObservableCollection<string> _FieldsList;
        public ObservableCollection<string> FieldsList
        {
            get
            {
                return _FieldsList ?? (_FieldsList = new ObservableCollection<string>());
            }
            set
            {
                if (_FieldsList != value)
                {
                    _FieldsList = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(FieldsList)));
                    }
                }
            }
        }
        public ObservableCollection<string> FunctionsList
        {
            get
            {
                return new ObservableCollection<string>() { "Abs(  )", "Atn(  )" };
            }
        }

        private string _SelectFunction;
        public string SelectFunction
        {
            get
            {
                return _SelectFunction;
            }
            set
            {
                if (_SelectFunction != value)
                {
                    _SelectFunction = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectFunction)));
                    }
                }
            }
        }

        private string _SelectedField;
        public string SelectedField
        {
            get
            {
                return _SelectFunction;
            }
            set
            {
                if (_SelectedField != value)
                {
                    _SelectedField = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedField)));
                    }
                }
            }
        }

        private string _CalculateField;
        public string CalculateField
        {
            get
            {
                return _CalculateField;
            }
            set
            {
                if (_CalculateField != value)
                {
                    _CalculateField = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(CalculateField)));
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public FieldCalculatorDlg(IFeatureLayer pFeaLayer)
        {
            InitializeComponent();
            this.DataContext = this;
            m_CurrentFeaSet = (pFeaLayer as FeatureLayer).FeatureSet;
            foreach (DataColumn col in m_CurrentFeaSet.DataTable.Columns)
            {
                FieldsList.Add(col.ColumnName);
            }
        }

        private void btnOpe_Click(object sender, RoutedEventArgs e)
        {
            string pBtnContent = (sender as Button).Content.ToString();
            string srcText = this.txtExpression.Text;
            int current = this.txtExpression.SelectionStart;
            this.txtExpression.Text = srcText.Substring(0, current) + pBtnContent + srcText.Substring(current, srcText.Length - current);
            this.txtExpression.SelectionStart = current + pBtnContent.Length;
            this.txtExpression.Focus();
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
            if (string.IsNullOrEmpty(this.CalculateField))
            {
                MessageBox.Show("Please select a calculate field");
                return;
            }

            ProgressBox p = new ProgressBox(0, 100, "Faying surface check progress");
            p.ShowPregress();
            p.SetProgressValue(0);
            p.SetProgressDescription("calculate...");
            double pi = Math.Round((double)(1.0 * 100 / m_CurrentFeaSet.Features.Count));
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
            this.btnClear.IsEnabled = true;
            string text = ("[" + this.lstFields.SelectedItem.ToString() + "]");
            string srcText = this.txtExpression.Text;
            int current = this.txtExpression.SelectionStart;
            this.txtExpression.Text = srcText.Substring(0, current) + text + srcText.Substring(current, srcText.Length - current);
            this.txtExpression.SelectionStart = current + text.Length;
            this.txtExpression.Focus();
        }

        private void lstFunctions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.lstFunctions.SelectedItem == null) return;
            this.btnClear.IsEnabled = true;
            string text = this.lstFunctions.SelectedItem.ToString();
            string srcText = this.txtExpression.Text;
            int current = this.txtExpression.SelectionStart;
            this.txtExpression.Text = srcText.Substring(0, current) + text + srcText.Substring(current, srcText.Length - current);
            this.txtExpression.SelectionStart = current + text.Length-2;
            this.txtExpression.Focus();
        }

        private void btnCreateField_Click(object sender, RoutedEventArgs e)
        {
            CreateFieldDlg f = new CreateFieldDlg(new List<string>(this.FieldsList));
            {
                if (f.ShowDialog() == true)
                {
                    m_NewFieldName = f.FieldName;
                    m_FieldType = f.Fieldtype;
                    this.btnCreateField.Content = m_NewFieldName;
                    this.CalculateField = m_NewFieldName;
                }
            }
        }

        private void ckxCreate_Checked(object sender, RoutedEventArgs e)
        {
            this.cboCalFields.IsEnabled = false;
            if (!string.IsNullOrEmpty(this.m_NewFieldName))
                this.CalculateField = m_NewFieldName;
            else
                this.CalculateField = "";
        }

        private void ckxCreate_Unchecked(object sender, RoutedEventArgs e)
        {
            this.cboCalFields.IsEnabled = true;
            this.CalculateField = this.cboCalFields.Text;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FieldsList.Count > 0)
            {
                this.cboCalFields.SelectedIndex = 0;
                this.CalculateField = this.cboCalFields.Text;
            }
        }
    }
}
