using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using RhinoInside.Revit.Convert.Geometry;
using RhinoInside.Revit.External.DB.Extensions;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components.Element.Rebar
{
  public class RebarCreateFromCurves : ReconstructElementComponent
  {
    public override Guid ComponentGuid => new Guid("E119CAD1-2C0A-4451-AC26-21B0D7A5F14B");
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    public RebarCreateFromCurves() : base
    (
      "Add Rebar", "Rebar",
      "Given its curves and host element it adds a Rebar element to the active Revit document",
      "Revit", "Build"
    )
    { }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      manager.AddParameter(new Parameters.Element(), "Rebar", "R", "New Rebar", GH_ParamAccess.item);
    }

    protected override void OnAfterStart(DB.Document document, string strTransactionName)
    {
      base.OnAfterStart(document, strTransactionName);
    }

    void ReconstructRebarCreateFromCurves
    (
      DB.Document doc,
      ref DB.Structure.Rebar element,
      DB.HostObject host,
      Rhino.Geometry.Curve curve
    )
    {
      var centerLine = curve.ToCurve();
      List<CurveLoop> curves = new List<CurveLoop>() { CurveLoop.Create(new List<DB.Curve>() { centerLine }) };
      // Reconstruct Rebar
      {
        var rebarBarType = doc.GetElement(DB.Structure.RebarBarType.CreateDefaultRebarBarType(doc)) as DB.Structure.RebarBarType;
        DB.Structure.RebarFreeFormValidationResult validationResult;
        var newRebar = DB.Structure.Rebar.CreateFreeForm
          (
          doc,
          rebarBarType,
          host,
          curves,
          out validationResult
          );

        newRebar.Document.Regenerate();

        var parametersMask = new DB.BuiltInParameter[]
        {
        };

        ReplaceElement(ref element, newRebar, parametersMask);
      }
    }
  }
}
