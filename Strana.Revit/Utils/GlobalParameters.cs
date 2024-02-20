using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.Utils
{
    public static class GlobalParameters
    {
        public static string LinkInfo { get; set; }
        public static string SectionName { get; set; }
        public static string Date
        {
            get
            {
                return DateTime.Now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

    }
}
