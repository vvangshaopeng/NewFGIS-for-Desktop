using DotSpatial.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotSpatialGISManager
{
    public class MeasureModel
    {
        public string ID
        {
            get; set;
        }

        public string Layer
        {
            get; set;
        }

        public string Length
        {
            get; set;
        }

        public string Area
        {
            get; set;
        }

        public string Shape
        {
            get; set;
        }

        public MeasureModel(string id, string layer, string length, string area, string shape)
        {
            ID = id;
            Layer = layer;
            Length = length;
            Area = area;
            Shape = shape;
        }
    }

    public class LocationLayerModel
    {
        public string ImagePath { get; set; }

        private bool _ischecked;
        public bool IsChecked
        {
            get
            {
                return _ischecked;
            }
            set
            {
                if (_ischecked != value)
                {
                    _ischecked = value;
                    if (UCControls.UCQuery.m_LocationQueryDlg != null)
                    {
                        LocationQueryDlg f = UCControls.UCQuery.m_LocationQueryDlg;
                        f.ResetSourceLayers();
                    }
                }
            }
        }

        public string LayerName { get; set; }

        public FeatureLayer Layer { get; set; }
    }
}
