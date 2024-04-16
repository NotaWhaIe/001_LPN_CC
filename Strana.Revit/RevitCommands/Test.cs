using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using FirstRevitPlugin.FailuresProcessing;

using Strana.Revit.HoleTask.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Controls;
//using Autodesk.DesignScript.Geometry;

namespace Strana.Revit.HoleTask.RevitCommands
{
    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        static AddInId addinId = new AddInId(new Guid("f64706dd-e8f6-4cbe-9cc6-a2910be5ad5a"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Переопределение графики для окон и витражей без марок");

                // Изменяем имя переменной с 'elements' на 'filteredElements'
                IEnumerable<Element> filteredElements = new FilteredElementCollector(doc)
                    .WherePasses(new ElementMulticlassFilter(new List<Type> { typeof(Wall), typeof(FamilyInstance) }))
                    .Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows ||
                                (e is Wall w && w.WallType.Kind == WallKind.Curtain));

                OverrideGraphicSettings settingsToApply = new OverrideGraphicSettings();
                settingsToApply.SetProjectionLineColor(new Color(255, 0, 0)); // Красный цвет для линий проекции
                settingsToApply.SetCutLineColor(new Color(255, 0, 0)); // Красный цвет для линий разреза

                OverrideGraphicSettings settingsToRemove = new OverrideGraphicSettings(); // Пустые настройки для снятия переопределения

                foreach (Element elem in filteredElements)
                {
                    // Получаем теги для элемента на активном видеф
                    var tags = new FilteredElementCollector(doc, doc.ActiveView.Id)
                        .OfClass(typeof(IndependentTag))
                        .Cast<IndependentTag>()
                        .Where(t => t.TaggedLocalElementId == elem.Id);

                    // Если теги есть, снимаем переопределение графики с элемента
                    if (tags.Any())
                    {
                        doc.ActiveView.SetElementOverrides(elem.Id, settingsToRemove);
                    }
                    else
                    {
                        // Иначе применяем настройки переопределения графики
                        doc.ActiveView.SetElementOverrides(elem.Id, settingsToApply);
                    }
                }

                tx.Commit();
                return Result.Succeeded;
            }
        }
    }
}
