using Autodesk.Revit.DB;
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
        public static string UserName { get; set; }
        public static string OldTasksWall { get; set; }
        public static string OldTasksFloor { get; set; }
        public static string СreatedTasksWall { get; set; }
        public static string СreatedTasksFloor { get; set; }
        public static string DeletedTasks { get; set; }
        public static List<FamilyInstance> ЕxistingTask { get; set; } = new List<FamilyInstance>();
        public static List<FamilyInstance> ЕxistingTaskWall { get; set; } = new List<FamilyInstance>();
        public static List<FamilyInstance> ЕxistingTaskFloor { get; set; } = new List<FamilyInstance>();

        public static void SetScriptCreationMethod(FamilyInstance instance)
        {
            Parameter param = instance.LookupParameter("SD_Способ создания задания");
            if (param != null && !param.IsReadOnly)
            {
                param.Set("СКРИПТ");
            }
        }

        public static string Date
        {
            get
            {
                return DateTime.Now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        public static void ResetParameters()
        {
            LinkInfo = null;
            SectionName = null;
            UserName = null;
            OldTasksWall = null;
            OldTasksFloor = null;
            СreatedTasksWall = null;
            СreatedTasksFloor = null;
            DeletedTasks = null;
            ЕxistingTask = new List<FamilyInstance>();
            ЕxistingTaskWall = new List<FamilyInstance>();
            ЕxistingTaskFloor = new List<FamilyInstance>();
        }
    }
}
