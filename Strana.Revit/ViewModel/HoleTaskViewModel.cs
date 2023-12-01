using Autodesk.Revit.UI;
using Strana.Revit.NavisReportViewer.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Strana.Revit.HoleTask.ViewModel
{
    internal class HoleTaskViewModel : BaseViewModel
    {
        private int offSetJoin = Confing.Default.offSetJoin;

        public int OffSetJoin
        {
            get
            {
                return offSetJoin;
            }
            set
            {
                OnPropertyChanged(nameof(offSetJoin));
                if (value is int)
                {
                    offSetJoin = value;
                    Confing.Default.offSetJoin = offSetJoin;
                    Confing.Default.Save();
                }
            }
        }
        public ICommand RunScriptCommand => new RouteCommands(() => this.runScript());

        private void runScript()
        {
            TaskDialog.Show("asdfasdf", "asdf");
        }
    }
}
