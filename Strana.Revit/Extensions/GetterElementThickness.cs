using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.Extensions
{
    public static class GetterElementThickness
    {
        public static double GetInterctedElementThickness(this Element intersectedElement)
        {
            if (intersectedElement.GetType() == typeof(Wall))
            {
                Wall wall = intersectedElement as Wall;
                double wallThickness = wall.Width;
                return wallThickness;
            }
            else
            {
                double floorThickness = intersectedElement.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();
                return floorThickness;
            }
        }

    }
}
