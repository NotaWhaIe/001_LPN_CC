using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.ViewModel
{
    public static class WpfSettings
    {
        public static int MinMepElementLength
        {
            get { return Confing.Default.minMepElementLength; }
        }
        public static int MinMepElementSize
        {
            get { return Confing.Default.minMepElementSize; }
        }
        public static int OffSetJoin
        {
            get { return Confing.Default.offSetJoin; }
        }
        public static int OffSetHoleTask
        {
            get { return Confing.Default.offSetHoleTask; }
        }
        public static int RoundHoleTaskDimensions
        {
            get { return Confing.Default.roundHoleTaskDimensions; }
        }
        public static int RoundHoleTaskInPlane
        {
            get { return Confing.Default.roundHoleTaskInPlane; }
        }
    }
}
