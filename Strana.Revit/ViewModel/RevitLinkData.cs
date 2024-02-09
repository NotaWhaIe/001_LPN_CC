using Strana.Revit.NavisReportViewer.ViewModels.Base;

namespace Strana.Revit.HoleTask.ViewModel
{
    public class RevitLinkData : BaseViewModel
    {
        public RevitLinkData(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; } // Сделал свойство только для чтения с возможностью установки в конструкторе

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value) // Проверка на изменение значения, чтобы избежать ненужных вызовов
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected)); // Исправлено на имя публичного свойства

                    // Обновление конфигурации при изменении состояния выбора
                    if (isSelected && !Confing.Default.revitLinks.Contains(this.Name))
                    {
                        Confing.Default.revitLinks += this.Name + ";";
                    }
                    else
                    {
                        Confing.Default.revitLinks = Confing.Default.revitLinks.Replace(this.Name + ";", string.Empty);
                    }
                    Confing.Default.Save();
                }
            }
        }
    }
}
