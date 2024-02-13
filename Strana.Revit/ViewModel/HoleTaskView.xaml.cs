using Autodesk.Revit.DB;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
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
    public partial class HoleTaskView : Window
    {
        private bool shouldExecuteProgram = false;
        private bool isMagicButtonPressed = false;


        public HoleTaskView(Document doc)
        {
            this.DataContext = new HoleTaskViewModel(doc);
            ColorZoneAssist.SetMode(new CheckBox(), ColorZoneMode.Dark);
            Hue hue = new Hue("xyz", System.Windows.Media.Color.FromArgb(1, 2, 3, 4), System.Windows.Media.Color.FromArgb(1, 5, 6, 7));
            InitializeComponent();
        }

        public bool ShouldExecuteProgram
        {
            get { return shouldExecuteProgram; }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Если окно закрывается обычным способом, программа не выполняется
            if (!isMagicButtonPressed)
            {
                shouldExecuteProgram = false;
            }
        }

        public void RunProgram(object sender, RoutedEventArgs e)
        {
            // Указываем, что кнопка для запуска программы была нажата
            isMagicButtonPressed = true;

            // Получаем ViewModel из DataContext
            if (this.DataContext is HoleTaskViewModel viewModel)
            {
                // Вызываем метод ViewModel для обработки выбранных связей
                viewModel.ProcessSelectedLinks();
            }

            // Подготавливаем окно к закрытию
            shouldExecuteProgram = true;
            this.Close(); // Закрываем окно
        }
    }
}