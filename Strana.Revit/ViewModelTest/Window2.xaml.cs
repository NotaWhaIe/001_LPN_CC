using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

using Autodesk.Revit.DB;

namespace Strana.Revit.ViewModelTest
{
    public partial class Window2 : Window, INotifyPropertyChanged
    {
        public List<string> FamilyInstances2aNames { get; set; }
        public List<string> FamilyInstances2bNames { get; set; }
        public List<string> FamilySymbols3aNames { get; set; }
        public List<string> FamilySymbols3bNames { get; set; }
        public List<DisplayFamilyInstance> SelectedFamilyInstances { get; set; }

        private List<DisplayFamilyInstance> _familyInstances2a;
        private List<DisplayFamilyInstance> _familyInstances2b;
        private List<DisplayFamilySymbol> _familySymbols3a;
        private List<DisplayFamilySymbol> _familySymbols3b;

        public Window2(List<DisplayFamilyInstance> familyInstances2a, List<DisplayFamilyInstance> familyInstances2b, List<DisplayFamilySymbol> familySymbols3a, List<DisplayFamilySymbol> familySymbols3b)
        {
            InitializeComponent();
            DataContext = this;
            _familyInstances2a = familyInstances2a;
            _familyInstances2b = familyInstances2b;
            _familySymbols3a = familySymbols3a;
            _familySymbols3b = familySymbols3b;

            FamilyInstances2aNames = familyInstances2a
                .Select(fi => $"{fi.FamilyName} -- {GetFamilyTypeName(fi.Instance.Document, fi.Instance.GetTypeId())}")
                .OrderBy(name => name, StringComparer.Create(new System.Globalization.CultureInfo("en-US"), false))
                .ThenBy(name => name, StringComparer.Create(new System.Globalization.CultureInfo("ru-RU"), false))
                .ToList();
            FamilyInstances2bNames = familyInstances2b
                .Select(fi => $"{fi.FamilyName} -- {GetFamilyTypeName(fi.Instance.Document, fi.Instance.GetTypeId())}")
                .OrderBy(name => name, StringComparer.Create(new System.Globalization.CultureInfo("en-US"), false))
                .ThenBy(name => name, StringComparer.Create(new System.Globalization.CultureInfo("ru-RU"), false))
                .ToList();
            FamilySymbols3aNames = familySymbols3a
                .Select(fs => $"{fs.FamilyName} -- {fs.TypeName}")
                .OrderBy(name => name, StringComparer.Create(new System.Globalization.CultureInfo("en-US"), false))
                .ThenBy(name => name, StringComparer.Create(new System.Globalization.CultureInfo("ru-RU"), false))
                .ToList();
            FamilySymbols3bNames = familySymbols3b
                .Select(fs => $"{fs.FamilyName} -- {fs.TypeName}")
                .OrderBy(name => name, StringComparer.Create(new System.Globalization.CultureInfo("en-US"), false))
                .ThenBy(name => name, StringComparer.Create(new System.Globalization.CultureInfo("ru-RU"), false))
                .ToList();
            SelectedFamilyInstances = new List<DisplayFamilyInstance>();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var selected2aNames = familyInstances2aList.SelectedItems.Cast<string>().ToList();
            var selected2bNames = familyInstances2bList.SelectedItems.Cast<string>().ToList();
            var selected3aNames = familySymbols3aList.SelectedItems.Cast<string>().ToList();
            var selected3bNames = familySymbols3bList.SelectedItems.Cast<string>().ToList();

            SelectedFamilyInstances.AddRange(_familyInstances2a.Where(fi => selected2aNames.Contains($"{fi.FamilyName} -- {GetFamilyTypeName(fi.Instance.Document, fi.Instance.GetTypeId())}")));
            SelectedFamilyInstances.AddRange(_familyInstances2b.Where(fi => selected2bNames.Contains($"{fi.FamilyName} -- {GetFamilyTypeName(fi.Instance.Document, fi.Instance.GetTypeId())}")));
            SelectedFamilyInstances.AddRange(_familySymbols3a.Where(fs => selected3aNames.Contains($"{fs.FamilyName} -- {fs.TypeName}")).Select(fs => new DisplayFamilyInstance { FamilyName = fs.FamilyName, TypeName = fs.TypeName, Instance = null }));
            SelectedFamilyInstances.AddRange(_familySymbols3b.Where(fs => selected3bNames.Contains($"{fs.FamilyName} -- {fs.TypeName}")).Select(fs => new DisplayFamilyInstance { FamilyName = fs.FamilyName, TypeName = fs.TypeName, Instance = null }));

            DialogResult = true;
            Close();
        }

        private string GetFamilyTypeName(Document doc, ElementId typeId)
        {
            FamilySymbol familySymbol = doc.GetElement(typeId) as FamilySymbol;
            return familySymbol?.Name ?? "Unknown Type";
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
