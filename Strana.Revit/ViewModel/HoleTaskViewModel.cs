using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Strana.Revit.NavisReportViewer.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Strana.Revit.Extension;


namespace Strana.Revit.HoleTask.ViewModel
{
    internal class HoleTaskViewModel : BaseViewModel
    {
        private bool areRoundHoleTaskInPlane = Confing.Default.areRoundHoleTaskInPlane;
        private int roundHoleTaskInPlane = Confing.Default.roundHoleTaskInPlane;
        private bool areRoundHoleTaskDimensions = Confing.Default.areRoundHoleTaskDimensions;
        private int roundHoleTaskDimensions = Confing.Default.roundHoleTaskDimensions;
        private int offSetHoleTask = Confing.Default.offSetHoleTask;
        private bool areJoin = Confing.Default.areJoin;
        private int offSetJoin = Confing.Default.offSetJoin;
        private bool arePlaceHoleTaskInOpenings = Confing.Default.arePlaceHoleTaskInOpenings;
        private bool arePickUpElements = Confing.Default.arePickUpElements;

        public List<RevitLinkData> LinkedFileData { get; set; }

        public HoleTaskViewModel(Document doc)
        {
            this.currentDocument = doc;
            LinkedFileData = GetLinkedFileNames();
        }

        public Document currentDocument { get; set; }


        public List<RevitLinkData> GetLinkedFileNames()
        {
            List<string> linkedFileNames = this.currentDocument.GetAllRevitLinkedFileNames();
            List< RevitLinkData> rvtlinksData = new List<RevitLinkData>();
            foreach(string name in  linkedFileNames) 
            {
                rvtlinksData.Add(new RevitLinkData(name.Split(':')[0]));
            }

            return rvtlinksData;
        }


        public bool AreRoundHoleTaskInPlane
        {
            get { return roundHoleTaskInPlane != 0; }
            set
            {
                OnPropertyChanged(nameof(AreRoundHoleTaskInPlane));
                roundHoleTaskInPlane = value ? 1 : 0;
                Confing.Default.roundHoleTaskInPlane = roundHoleTaskInPlane;
                Confing.Default.Save();
            }
        }
        public int RoundHoleTaskInPlane
        {
            get
            {
                return roundHoleTaskInPlane;
            }
            set
            {
                OnPropertyChanged(nameof(RoundHoleTaskInPlane));
                if (value is int)
                {
                    roundHoleTaskInPlane = value;
                    Confing.Default.roundHoleTaskInPlane = roundHoleTaskInPlane;
                    Confing.Default.Save();
                }
            }
        }
        public bool AreRoundHoleTaskDimensions
        {
            get { return roundHoleTaskInPlane != 0; }
            set
            {
                OnPropertyChanged(nameof(AreRoundHoleTaskDimensions));
                roundHoleTaskInPlane = value ? 1 : 0;
                Confing.Default.areRoundHoleTaskDimensions = areRoundHoleTaskDimensions;
                Confing.Default.Save();
            }
        }
        public int RoundHoleTaskDimensions
        {
            get
            {
                return roundHoleTaskDimensions;
            }
            set
            {
                OnPropertyChanged(nameof(RoundHoleTaskDimensions));
                if (value is int)
                {
                    roundHoleTaskDimensions = value;
                    Confing.Default.roundHoleTaskDimensions = roundHoleTaskDimensions;
                    Confing.Default.Save();
                }
            }
        }
        public int OffSetHoleTask
        {
            get
            {
                return offSetHoleTask;
            }
            set
            {
                OnPropertyChanged(nameof(OffSetHoleTask));
                if (value is int)
                {
                    offSetHoleTask = value;
                    Confing.Default.offSetHoleTask = offSetHoleTask;
                    Confing.Default.Save();
                }
            }
        }
        public bool AreJoin
        {
            get { return roundHoleTaskInPlane != 0; }
            set
            {
                OnPropertyChanged(nameof(AreJoin));
                roundHoleTaskInPlane = value ? 1 : 0;
                Confing.Default.areJoin = areJoin;
                Confing.Default.Save();
            }
        }
        public int OffSetJoin
        {
            get
            {
                return offSetJoin;
            }
            set
            {
                OnPropertyChanged(nameof(OffSetJoin));
                if (value is int)
                {
                    offSetJoin = value;
                    Confing.Default.offSetJoin = offSetJoin;
                    Confing.Default.Save();
                }
            }
        }
        public bool ArePlaceHoleTaskInOpenings
        {
            get { return roundHoleTaskInPlane != 0; }
            set
            {
                OnPropertyChanged(nameof(ArePlaceHoleTaskInOpenings));
                roundHoleTaskInPlane = value ? 1 : 0;
                Confing.Default.arePlaceHoleTaskInOpenings = arePlaceHoleTaskInOpenings;
                Confing.Default.Save();
            }
        }
        public bool ArePickUpElements
        {
            get { return roundHoleTaskInPlane != 0; }
            set
            {
                OnPropertyChanged(nameof(ArePickUpElements));
                roundHoleTaskInPlane = value ? 1 : 0;
                Confing.Default.arePickUpElements = arePickUpElements;
                Confing.Default.Save();
            }
        }



        public ICommand RunScriptCommand => new RouteCommands(() => this.runScript());

        private void runScript()
        {
            TaskDialog.Show("asdfasdf", "asdf");
        }
    }
}
