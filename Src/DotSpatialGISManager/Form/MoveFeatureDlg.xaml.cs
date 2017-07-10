using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
using GeoAPI.Geometries;
using Microsoft.Win32;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DotSpatialGISManager
{
    /// <summary>
    /// MoveFeatureDlg.xaml 的交互逻辑
    /// </summary>
    public partial class MoveFeatureDlg : Window, INotifyPropertyChanged
    {
        private IFeatureLayer[] m_FeaLyrList = new IFeatureLayer[] { };
        private IFeatureLayer m_CurrentFeaLyr = null;
        private IFeatureSet m_CurrentFeaSet = null;
        private IFeatureSet m_ResultFeaset = null;
        private IFeature feaMove = null;

        #region 绑定
        private List<Coordinate> _CoordList;
        public List<Coordinate> CoordList
        {
            get
            {
                return _CoordList ?? (_CoordList = new List<Coordinate>());
            }
            set
            {
                _CoordList = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public MoveFeatureDlg()
        {
            InitializeComponent();
            this.DataContext = this;
            m_FeaLyrList = MainWindow.m_DotMap.GetFeatureLayers();

            foreach (ILayer layer in m_FeaLyrList)
            {
                if (layer is IFeatureLayer)
                    this.cboLayer.Items.Add((layer as FeatureLayer).Name);
            }
            if (this.cboLayer.Items.Count > 0)
            {
                this.cboLayer.SelectedIndex = 0;
            }
        }

        private void cboLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex == -1) return;
            m_CurrentFeaLyr = m_FeaLyrList[this.cboLayer.SelectedIndex];
            //直接 m_CurrentFeaLyr as IFeatureSet; Wrong!
            m_CurrentFeaSet = (m_CurrentFeaLyr as FeatureLayer).FeatureSet;
            MainWindow.m_DotMap.FunctionMode = FunctionMode.Select;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //若未选择图层
            if (this.cboLayer.Text == "")
            {
                MessageBox.Show("Please select a layer！");
                return;
            }
            if (m_CurrentFeaLyr.Selection.Count == 0 || m_CurrentFeaLyr.Selection.Count > 1)
            {
                MessageBox.Show("Please select one feature");
                m_CurrentFeaLyr.UnSelectAll();
                return;
            }

            IFeature pFeature = m_CurrentFeaLyr.Selection.ToFeatureList()[0];
            feaMove = (from u in (m_CurrentFeaLyr as FeatureLayer).FeatureSet.Features
                       where u.Geometry.Intersects(pFeature.Geometry)
                       select u).FirstOrDefault();

            double moveX = 0;
            double moveY = 0;
            try
            {
                moveX = Convert.ToDouble(this.txtMoveX.Text);
                moveY = Convert.ToDouble(this.txtMoveY.Text);
            }
            catch
            {
                MessageBox.Show("input a number");
            }
            Coordinate moveCoord = new Coordinate();
            moveCoord.X = moveX;
            moveCoord.Y = moveY;
            Coordinate resultCoord = new Coordinate();
            if (feaMove.FeatureType == FeatureType.Point)
            {
                m_ResultFeaset = new FeatureSet(FeatureType.Line);
                foreach (DataColumn column in m_CurrentFeaSet.DataTable.Columns)
                {
                    DataColumn col = new DataColumn(column.ColumnName, column.DataType);
                    m_ResultFeaset.DataTable.Columns.Add(col);
                }

                //move
                resultCoord.X = feaMove.Geometry.Coordinate.X + moveCoord.X;
                resultCoord.Y = feaMove.Geometry.Coordinate.Y + moveCoord.Y;
                IPoint pPoint = new NetTopologySuite.Geometries.Point(resultCoord);
                IFeature lFeaM = m_ResultFeaset.AddFeature(pPoint);
                for (int i = 0; i < feaMove.DataRow.ItemArray.Count(); i++)
                {
                    lFeaM.DataRow[i] = feaMove.DataRow[i];
                }

                m_CurrentFeaSet.Features.Remove(feaMove);
                foreach (var fea in m_CurrentFeaSet.Features)
                {
                    IFeature lFea = m_ResultFeaset.AddFeature(fea.Geometry);
                    for (int i = 0; i < fea.DataRow.ItemArray.Count(); i++)
                    {
                        lFea.DataRow[i] = fea.DataRow[i];
                    }
                }
                m_ResultFeaset.InitializeVertices();
                MainWindow.m_DotMap.ResetBuffer();
                m_CurrentFeaSet.AddFeature(feaMove.Geometry);

                m_ResultFeaset.Projection = MainWindow.m_DotMap.Projection;
                m_ResultFeaset.Name = m_CurrentFeaSet.Name;
                m_CurrentFeaLyr = MainWindow.m_DotMap.Layers.Add(m_ResultFeaset);
                m_CurrentFeaLyr.LegendText = m_ResultFeaset.Name + "_copy";
                CoordList.Clear();
            }
            else if (feaMove.FeatureType == FeatureType.Line)
            {
                //create a new featureset
                m_ResultFeaset = new FeatureSet(FeatureType.Line);
                foreach (DataColumn column in m_CurrentFeaSet.DataTable.Columns)
                {
                    DataColumn col = new DataColumn(column.ColumnName, column.DataType);
                    m_ResultFeaset.DataTable.Columns.Add(col);
                }

                //move
                for (int i = 0; i < feaMove.Geometry.NumPoints; i++)
                {
                    feaMove.Geometry.Coordinates[i].X += moveCoord.X;
                    feaMove.Geometry.Coordinates[i].Y += moveCoord.Y;
                    CoordList.Add(feaMove.Geometry.Coordinates[i]);
                }
                ILineString pLine = new LineString(CoordList.ToArray());

                IFeature lFeaM = m_ResultFeaset.AddFeature(pLine);
                for (int i = 0; i < feaMove.DataRow.ItemArray.Count(); i++)
                {
                    lFeaM.DataRow[i] = feaMove.DataRow[i];
                }

                m_CurrentFeaLyr.InvertSelection();
                IFeatureSet fs = m_CurrentFeaLyr.Selection.ToFeatureSet();
                foreach (var fea in fs.Features)
                {
                    IFeature lFea = m_ResultFeaset.AddFeature(fea.Geometry);
                    for (int i = 0; i < fea.DataRow.ItemArray.Count(); i++)
                    {
                        lFea.DataRow[i] = fea.DataRow[i];
                    }
                }
                m_ResultFeaset.InitializeVertices();
                MainWindow.m_DotMap.ResetBuffer();

                m_ResultFeaset.Projection = MainWindow.m_DotMap.Projection;
                m_ResultFeaset.Name = m_CurrentFeaSet.Name;
                m_CurrentFeaLyr = MainWindow.m_DotMap.Layers.Add(m_ResultFeaset);
                m_CurrentFeaLyr.LegendText = m_ResultFeaset.Name + "_copy";
                CoordList.Clear();
            }
            else if (feaMove.FeatureType == FeatureType.Polygon)
            {
                //create a new featureset
                m_ResultFeaset = new FeatureSet(FeatureType.Line);
                foreach (DataColumn column in m_CurrentFeaSet.DataTable.Columns)
                {
                    DataColumn col = new DataColumn(column.ColumnName, column.DataType);
                    m_ResultFeaset.DataTable.Columns.Add(col);
                }

                //move
                for (int i = 0; i < feaMove.Geometry.NumPoints; i++)
                {
                    feaMove.Geometry.Coordinates[i].X += moveCoord.X;
                    feaMove.Geometry.Coordinates[i].Y += moveCoord.Y;
                    CoordList.Add(feaMove.Geometry.Coordinates[i]);
                }
                ILinearRing LineRing = new LinearRing(CoordList.ToArray());
                IPolygon pPolygon = new NetTopologySuite.Geometries.Polygon(LineRing);
                IFeature lFeaM = m_ResultFeaset.AddFeature(pPolygon);
                for (int i = 0; i < feaMove.DataRow.ItemArray.Count(); i++)
                {
                    lFeaM.DataRow[i] = feaMove.DataRow[i];
                }

                m_CurrentFeaLyr.InvertSelection();
                IFeatureSet fs = m_CurrentFeaLyr.Selection.ToFeatureSet();
                foreach (var fea in fs.Features)
                {
                    IFeature lFea = m_ResultFeaset.AddFeature(fea.Geometry);
                    for (int i = 0; i < fea.DataRow.ItemArray.Count(); i++)
                    {
                        lFea.DataRow[i] = fea.DataRow[i];
                    }
                }
                m_ResultFeaset.InitializeVertices();
                MainWindow.m_DotMap.ResetBuffer();

                m_ResultFeaset.Projection = MainWindow.m_DotMap.Projection;
                m_ResultFeaset.Name = m_CurrentFeaSet.Name;
                m_CurrentFeaLyr = MainWindow.m_DotMap.Layers.Add(m_ResultFeaset);
                m_CurrentFeaLyr.LegendText = m_ResultFeaset.Name + "_copy";
                CoordList.Clear();
            }
            MainWindow.m_DotMap.Refresh();
            IFeatureLayer[] m_FeaLyrList = MainWindow.m_DotMap.GetFeatureLayers();
            for (int i = 0; i < MainWindow.m_DotMap.Layers.Count; i++)
            {
                m_FeaLyrList[i].ClearSelection();
            }
            this.Close();
        }
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog f = new SaveFileDialog();
            f.AddExtension = true;
            f.Filter = "ShapeFile(*.shp)|*.shp";
            f.Title = "Select Save Path";
            if (f.ShowDialog() == true)
            {
                this.txtPath.Text = f.FileName;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UCControls.UCVectorDataEditing.m_MoveFeatureDlg = null;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
