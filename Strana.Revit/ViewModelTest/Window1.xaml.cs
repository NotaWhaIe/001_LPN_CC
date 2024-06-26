using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

using Autodesk.Revit.DB;
///work
namespace Strana.Revit.ViewModelTest
{
    public partial class Window1 : Window, INotifyPropertyChanged
    {
        public List<string> RevitLinkNames { get; set; }
        public List<RevitLinkInstance> SelectedLinks { get; set; }
        private List<RevitLinkInstance> _revitLinks;

        public Window1(List<RevitLinkInstance> revitLinks)
        {
            InitializeComponent();
            DataContext = this;
            _revitLinks = revitLinks;
            RevitLinkNames = revitLinks.Select(x => x.Name).ToList();
            SelectedLinks = new List<RevitLinkInstance>();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedNames = revitLinksList.SelectedItems.Cast<string>().ToList();
            SelectedLinks = _revitLinks.Where(x => selectedNames.Contains(x.Name)).ToList();
            DialogResult = true;
            Close();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
