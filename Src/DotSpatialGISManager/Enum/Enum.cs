using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotSpatialGISManager.Enum
{
    public enum FeaType
    {
        None = 0,
        Point,
        Polyline,
        Polygon,
    }

    public enum SelectionMethod
    {
        Select = 0,
        Add,
        RemoveFromSelected,
        SelectFromSelected,
    } 

    public enum SpatialMethod
    {
        Intersect = 0,
        Contains,
        Within,
    }
}
