using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
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
using Common;
using System.Data;
using DotSpatialGISManager.Enum;

namespace DotSpatialGISManager
{
    /// <summary>
    /// FayingSurfaceDlg.xaml 的交互逻辑
    /// </summary>
    public partial class CreateFieldDlg : Window
    {
        private List<string> m_FieldList;
        public CreateFieldDlg(List<string> FieldsList)
        {
            InitializeComponent();
            m_FieldList = FieldsList;
            this.cboType.Items.Add("String");
            this.cboType.Items.Add("Double");
            this.cboType.SelectedIndex = 0;
        }

        public string FieldName
        {
            get;
            private set;
        }

        public FieldType Fieldtype
        {
            get;
            private set;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtFieldName.Text == "")
            {
                MessageBox.Show("Please set a field name");
                return;
            }
            if (m_FieldList.Contains(this.txtFieldName.Text))
            {
                MessageBox.Show("The field already exists");
                return;
            }
            this.FieldName = this.txtFieldName.Text;
            if (this.cboType.SelectedIndex == 0)
                Fieldtype = FieldType.String;
            else
                Fieldtype = FieldType.Double;
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
