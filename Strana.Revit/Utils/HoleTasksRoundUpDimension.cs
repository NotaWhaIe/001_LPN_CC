﻿using Autodesk.Revit.DB;
using Strana.Revit.HoleTask.Extensions.RevitElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Strana.Revit.HoleTask.Utils
{
    internal class HoleTasksRoundUpDimension
    {
        private static bool areRoundHoleTaskDimensions => Confing.Default.areRoundHoleTaskDimensions;

        private static int roundHoleTaskDimensions => Confing.Default.roundHoleTaskDimensions;
        private const double FeetToMillimeters = 304.8;
        private const double MillimetersToFeet = 1 / FeetToMillimeters;

        internal static double RoundUpParameter(double parameterValue)
        {
            if (!areRoundHoleTaskDimensions)
            {
                return parameterValue;
            }

            double originalValueInMillimeters = parameterValue * FeetToMillimeters;
            double roundedValueInMillimeters = Math.Ceiling(originalValueInMillimeters / roundHoleTaskDimensions) * roundHoleTaskDimensions;
            double roundedValueInFeet = roundedValueInMillimeters * MillimetersToFeet;

            return roundedValueInFeet;
        }
    }
}