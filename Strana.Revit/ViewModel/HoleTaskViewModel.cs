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
using System.Reflection;


namespace Strana.Revit.HoleTask.ViewModel
{
    public class HoleTaskViewModel : BaseViewModel
    {
        private bool areRoundHoleTaskInPlane = Confing.Default.areRoundHoleTaskInPlane;
        private int roundHoleTaskInPlane = Confing.Default.roundHoleTaskInPlane;
        private bool areRoundHoleTaskDimensions = Confing.Default.areRoundHoleTaskDimensions;
        private int roundHoleTaskDimensions = Confing.Default.roundHoleTaskDimensions;
        private int offSetHoleTask = Confing.Default.offSetHoleTask;
        private bool areJoin = Confing.Default.areJoin;
        private int offSetJoin = Confing.Default.offSetJoin;
        private bool arePickUpElements = Confing.Default.arePickUpElements;
        private bool arePlaceHoleTaskInOpenings = Confing.Default.arePlaceHoleTaskInOpenings;
        private int minMepElementSize = Confing.Default.minMepElementSize;    
        private int minMepElementLength = Confing.Default.minMepElementLength;    

        public ObservableCollection<RevitLinkData> LinkedFileData { get; set; }
        
        public HoleTaskViewModel(Document doc)
        {
            this.currentDocument = doc;
            // Инициализация ObservableCollection с данными связанных файлов
            LinkedFileData = new ObservableCollection<RevitLinkData>(GetLinkedFileNames());
        }

        public  IEnumerable<RevitLinkData> GetSelectedLinks() =>LinkedFileData.Where(x => x.IsSelected);

        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();


        public Document currentDocument { get; set; }

        public List<RevitLinkData> GetLinkedFileNames()
        {
            List<string> linkedFileNames = this.currentDocument.GetAllRevitLinkedFileNames();
            List<RevitLinkData> rvtlinksData = new List<RevitLinkData>();
            foreach (string name in linkedFileNames)
            {
                rvtlinksData.Add(new RevitLinkData(name.Split(':')[0]));
            }

            return rvtlinksData;
        }

        public bool AreRoundHoleTaskInPlane
        {
            get => this.areRoundHoleTaskInPlane;
            set
            {
                this.areRoundHoleTaskInPlane = value;
                Confing.Default.areRoundHoleTaskInPlane = this.areRoundHoleTaskInPlane;
                Confing.Default.Save();
                OnPropertyChanged(nameof(this.areRoundHoleTaskInPlane));
            }
        }
        public int RoundHoleTaskInPlane
        {
            get => this.roundHoleTaskInPlane;
            set
            {
                if (value is int)
                {
                    this.roundHoleTaskInPlane = value;
                    Confing.Default.roundHoleTaskInPlane = this.roundHoleTaskInPlane;
                    Confing.Default.Save();
                }
                OnPropertyChanged(nameof(this.roundHoleTaskInPlane));
            }
        }
        public bool AreRoundHoleTaskDimensions
        {
            get => this.areRoundHoleTaskDimensions;
            set
            {
                this.areRoundHoleTaskDimensions = value;
                Confing.Default.areRoundHoleTaskDimensions = this.areRoundHoleTaskDimensions;
                Confing.Default.Save();
                OnPropertyChanged(nameof(this.areRoundHoleTaskDimensions));
            }
        }
        public int RoundHoleTaskDimensions
        {
            get => this.roundHoleTaskDimensions;
            set
            {
                if (value is int)
                {
                    this.roundHoleTaskDimensions = value;
                    Confing.Default.roundHoleTaskDimensions = this.roundHoleTaskDimensions;
                    Confing.Default.Save();
                }
                OnPropertyChanged(nameof(this.roundHoleTaskDimensions));
            }
        }
        public int OffSetHoleTask
        {
            get => offSetHoleTask;
            set
            {
                if (value is int)
                {
                    this.offSetHoleTask = value;
                    Confing.Default.offSetHoleTask = this.offSetHoleTask;
                    Confing.Default.Save();
                }
                OnPropertyChanged(nameof(this.offSetHoleTask));
            }
        }
        public bool AreJoin
        {
            get => this.areJoin;
            set
            {
                this.areJoin = value;
                Confing.Default.areJoin = this.areJoin;
                Confing.Default.Save();
                OnPropertyChanged(nameof(this.areJoin));
            }
        }
        public int OffSetJoin
        {
            get => this.offSetJoin;
            set
            {
                if (value is int)
                {
                    offSetJoin = value;
                    Confing.Default.offSetJoin = this.offSetJoin;
                    Confing.Default.Save();
                }
                OnPropertyChanged(nameof(this.offSetJoin));
            }
        }
        //public bool ArePickUpElements
        //{
        //    get => this.arePickUpElements;
        //    set
        //    {
        //        this.arePickUpElements = value;
        //        Confing.Default.arePickUpElements = this.arePickUpElements;
        //        Confing.Default.Save();
        //        OnPropertyChanged(nameof(this.arePickUpElements));
        //    }
        //}
        public bool ArePlaceHoleTaskInOpenings
        {
            get => this.arePlaceHoleTaskInOpenings;
            set
            {
                this.arePlaceHoleTaskInOpenings = value;
                Confing.Default.arePlaceHoleTaskInOpenings = this.arePlaceHoleTaskInOpenings;
                Confing.Default.Save();
                OnPropertyChanged(nameof(this.arePlaceHoleTaskInOpenings));
            }
        }
        public int MinMepElementSize
        {
            get => this.minMepElementSize;
            set
            {
                if (value is int)
                {
                    minMepElementSize = value;
                    Confing.Default.minMepElementSize = this.minMepElementSize;
                    Confing.Default.Save();
                }
                OnPropertyChanged(nameof(this.minMepElementSize));
            }
        }
        public int MinMepElementLength
        {
            get => this.minMepElementLength;
            set
            {
                if (value is int)
                {
                    minMepElementLength = value;
                    Confing.Default.minMepElementLength = this.minMepElementLength;
                    Confing.Default.Save();
                }
                OnPropertyChanged(nameof(this.minMepElementLength));
            }
        }


        ///test example
        public ICommand RunScriptCommand => new RouteCommands(() => this.runScript());

        public ICommand ProcessSelectedLinksCommand => new RelayCommand(ProcessSelectedLinks);

        public void ProcessSelectedLinks()
        {
            var selectedLinks = GetSelectedLinks();
            foreach (var link in selectedLinks)
            {
                // Здесь ваша логика обработки выбранных связей
                // Например, поиск RevitLinkInstance в документе Revit и выполнение операций с ним
            }
        }


        private void runScript()
        {
            TaskDialog.Show("asdfasdf", "asdf");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Здесь можно добавить вашу логику, которая должна выполняться при закрытии окна
        }

        internal void ExecuteProgram()
        {
            // Здесь добавьте логику, которая должна быть выполнена
            // Например, запуск процессов создания отверстий, расчетов и т.д.
        }
    }
}
