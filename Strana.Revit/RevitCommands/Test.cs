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
//using Autodesk.DesignScript.Geometry;

namespace Strana.Revit.HoleTask.RevitCommands
{
    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        static AddInId addinId = new AddInId(new Guid("f64706dd-e8f6-4cbe-9cc6-a2910be5ad5a"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;


            var loadedLinks = GetLinkedFiles(doc)
                .Where(link => RevitLinkType.IsLoaded(doc, link.Id))
                .Select(link => link.Name)
                .ToList();

            var debag=loadedLinks.Count;

            using (Transaction trans = new Transaction(doc, "test"))
            {
                TransactionHandler.SetWarningResolver(trans);
                trans.Start();
                trans.Commit();
            }

            return Result.Succeeded;
        }

        private static List<RevitLinkType> GetLinkedFiles(Document doc)
        {
            FilteredElementCollector collector = new(doc);
            var linkedElements = collector
                .OfClass(typeof(RevitLinkType))
                .OfType<RevitLinkType>();

            return linkedElements.ToList();
        }
    }
}

