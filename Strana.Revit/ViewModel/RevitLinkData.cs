using Strana.Revit.NavisReportViewer.ViewModels.Base;


namespace Strana.Revit.HoleTask.ViewModel
{
    public class RevitLinkData : BaseViewModel
    {
        public RevitLinkData(string name)
        {
            this.Name = name;
        }
        public string Name { get; }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                ///Ставил точку останова для проверки события
                OnPropertyChanged(nameof(isSelected));
                if (isSelected && !Confing.Default.revitLinks.Contains(this.Name))
                {
                    Confing.Default.revitLinks = Confing.Default.revitLinks + this.Name + ";";
                    Confing.Default.Save();
                }
                else
                {
                    Confing.Default.revitLinks = Confing.Default.revitLinks.Replace(this.Name + ";", string.Empty);
                    Confing.Default.Save();
                }
            }
        }

    }
}
