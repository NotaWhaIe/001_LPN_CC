using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Strana.Revit.RevitCommands
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab("Strana");
            _ = CreateRibbonPanel(application);
            return Result.Succeeded;
        }

        public RibbonPanel CreateRibbonPanel(UIControlledApplication application, string tabName = "Strana")
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "СС");
            AddPushButton(ribbonPanel, "Test", Assembly.GetExecutingAssembly().Location, "", "");
            return ribbonPanel;
        }

        public void AddPushButton(RibbonPanel ribbonPanel, string buttonName, string path, string linkToCommand, string toolTip)
        {
            var buttonData = new PushButtonData(buttonName, buttonName, path, linkToCommand);
            var button = ribbonPanel.AddItem(buttonData) as PushButton;
            button.ToolTip = toolTip;
            button.LargeImage = (ImageSource)new BitmapImage(new Uri(@"", UriKind.RelativeOrAbsolute));
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
