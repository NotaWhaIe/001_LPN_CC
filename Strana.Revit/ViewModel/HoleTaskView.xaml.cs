using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Strana.Revit.HoleTask.ViewModel
{
    /// <summary>
    /// Логика взаимодействия для HoleTaskView.xaml
    /// </summary>
    public partial class HoleTaskView : Window
    {
        public HoleTaskView(Document doc)
        {
            this.DataContext = new HoleTaskViewModel(doc);
            InitializeComponent();
        }

        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            //var t = this.DataContext as HoleTaskViewModel;
            this.Close();
        }
    }
}
