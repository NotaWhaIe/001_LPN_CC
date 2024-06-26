using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.DB;

namespace Strana.Revit.ViewModelTest
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public List<RevitLinkInstance> RevitLinks { get; set; }
        public List<RevitLinkInstance> SelectedLinks { get; set; }
        public List<FamilyInstance> FamilyInstances2a { get; set; }
        public List<FamilyInstance> FamilyInstances2b { get; set; }
        public List<FamilyInstance> FamilyInstances3a { get; set; }
        public List<FamilyInstance> FamilyInstances3b { get; set; }

        public MainWindow(List<RevitLinkInstance> revitLinks)
        {
            InitializeComponent();
            DataContext = this;
            RevitLinks = revitLinks;
            SelectedLinks = new List<RevitLinkInstance>();
            FamilyInstances2a = new List<FamilyInstance>();
            FamilyInstances2b = new List<FamilyInstance>();
            FamilyInstances3a = new List<FamilyInstance>();
            FamilyInstances3b = new List<FamilyInstance>();
        }

        public void UpdateFamilyInstances(List<FamilyInstance> familyInstances2a, List<FamilyInstance> familyInstances2b, List<FamilyInstance> familyInstances3a, List<FamilyInstance> familyInstances3b)
        {
            FamilyInstances2a = familyInstances2a;
            FamilyInstances2b = familyInstances2b;
            FamilyInstances3a = familyInstances3a;
            FamilyInstances3b = familyInstances3b;

            OnPropertyChanged(nameof(FamilyInstances2a));
            OnPropertyChanged(nameof(FamilyInstances2b));
            OnPropertyChanged(nameof(FamilyInstances3a));
            OnPropertyChanged(nameof(FamilyInstances3b));
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedLinks = RevitLinks.Where(x => ((ListBoxItem)revitLinksList.ItemContainerGenerator.ContainerFromItem(x)).IsSelected).ToList();
            if (SelectedLinks.Any())
            {
                // Обновить списки семейств
                var familyInstances2a = new List<FamilyInstance>();
                var familyInstances2b = new List<FamilyInstance>();
                var familyInstances3a = new List<FamilyInstance>();
                var familyInstances3b = new List<FamilyInstance>();

                foreach (var link in SelectedLinks)
                {
                    var doc = link.GetLinkDocument();
                    if (doc != null)
                    {
                        familyInstances2a.AddRange(new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                            .OfClass(typeof(FamilyInstance))
                            .Cast<FamilyInstance>()
                            .ToList());

                        familyInstances2b.AddRange(new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                            .OfClass(typeof(FamilyInstance))
                            .Cast<FamilyInstance>()
                            .ToList());
                    }
                }

                familyInstances3a = new FilteredElementCollector(SelectedLinks.First().Document)
                    .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .ToList();

                familyInstances3b = new FilteredElementCollector(SelectedLinks.First().Document)
                    .OfCategory(BuiltInCategory.OST_CommunicationDevices)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .ToList();

                UpdateFamilyInstances(familyInstances2a, familyInstances2b, familyInstances3a, familyInstances3b);
            }

            DialogResult = true;
            Close();
        }

        private void OkButton_Click2a(object sender, RoutedEventArgs e)
        {
            // Обработка выбранных семейств 2a
        }

        private void OkButton_Click2b(object sender, RoutedEventArgs e)
        {
            // Обработка выбранных семейств 2b
        }

        private void OkButton_Click3a(object sender, RoutedEventArgs e)
        {
            // Обработка выбранных семейств 3a
        }

        private void OkButton_Click3b(object sender, RoutedEventArgs e)
        {
            // Обработка выбранных семейств 3b
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
