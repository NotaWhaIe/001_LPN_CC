using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.ViewModel
{
    public static class WpfSettings
    {
        /// пподключить везде где используется
        /// булевые тоже добавить
        public static double MinMepElementLength
        {
            get { return Confing.Default.minMepElementLength; }
        }
        public static double MinMepElementSize
        {
            get { return Confing.Default.minMepElementSize; }
        }
        public static double OffSetJoin
        {
            get { return Confing.Default.offSetJoin; }
        }
        public static double OffSetHoleTask
        {
            get { return Confing.Default.offSetHoleTask; }
        }
        public static double RoundHoleTaskDimensions
        {
            get { return Confing.Default.roundHoleTaskDimensions; }
        }
        public static double RoundHoleTaskInPlane
        {
            get { return Confing.Default.roundHoleTaskInPlane; }
        }
    }
}
