// <copyright file="IntersectingElementExtension.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using Autodesk.Revit.DB;
using Strana.Revit.HoleTask.ElementCollections;
using Strana.Revit.HoleTask.Extension.RevitElement;
using Strana.Revit.HoleTask.Utils;

namespace Strana.Revit.HoleTask.Extensions
{
    public static class IntersectingElementExtension
    {
        /// <summary>
        /// Create hole tasks by all intersected element off RevitLinkInstance.
        /// </summary>
        /// <param name="intersectingElement"><seealso cref="Autodesk.Revit.DB.Element"/></param>
        /// <param name="linkInstance"><seealso cref="RevitLinkInstance"/></param>
        /// <returns> list off holetasks items by intersected element (wall or floor).</returns>
        public static List<FamilyInstance> CreateHoleTasksByIntersectedElements(this Element intersectingElement, RevitLinkInstance linkInstance)
        {
            List<FamilyInstance> intersectedItemHoleTasks = new();

            /// добавить
    //        List<FamilyInstance> intersectionWallRectangularCombineList01 = allFamilyInstances00
    //.Where(fi => fi.Name.ToString() == "(Отв_Задание)_Стены_Прямоугольное")
    //.ToList();
    //        List<FamilyInstance> intersectionFloorRectangularCombineList02 = allFamilyInstances00
    //            .Where(fi => fi.Name.ToString() == "(Отв_Задание)_Перекрытия_Прямоугольное")
    //            .ToList();


            if (intersectingElement.AreElementsHaveFaces())
            {
                Document doc = linkInstance.Document;
                Document linkDoc = linkInstance.GetLinkDocument();

                IEnumerable<Element> mepElements = MepElementCollections.AllMepElementsByBBox(doc, intersectingElement, linkInstance.GetTotalTransform());
                /// For test with only one pipe/duct/cable tray
                //if (intersectingElement.Id.IntegerValue == 4040632)
                //{
                Solid floorWallSolid = intersectingElement.GetSolidWithoutHoles(linkInstance);
                foreach (Element mepElement in mepElements)
                {
                    Curve mepCurve = (mepElement.Location as LocationCurve).Curve;
                    SolidCurveIntersectionOptions defOptions = new();
                    SolidCurveIntersection solidCurve = floorWallSolid.IntersectWithCurve(mepCurve, defOptions);

                    if (solidCurve != null && solidCurve.SegmentCount > 0)
                    {
                        HoleTaskCreator holeTaskCreator = new(doc);
                        FamilyInstance createdHoleTask = holeTaskCreator.PlaceHoleTaskFamilyInstance(mepElement, solidCurve, intersectingElement, linkDoc, linkInstance);
                        if (createdHoleTask is not null)
                        {
                            intersectedItemHoleTasks.Add(createdHoleTask);
                        }
                    }
                }
            }

            return intersectedItemHoleTasks;
        }
    }
}
