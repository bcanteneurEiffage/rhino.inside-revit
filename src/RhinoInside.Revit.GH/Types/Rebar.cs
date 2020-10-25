using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using RhinoInside.Revit.Convert.Geometry;
using RhinoInside.Revit.Geometry.Extensions;
using RhinoInside.Revit.External.DB.Extensions;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Types
{
  public class Rebar : Element
  {
    public override string TypeName => "Revit Rebar";
    public override string TypeDescription => "Represents a Revit rebar";
    protected override Type ScriptVariableType => typeof(DB.Structure.Rebar);
    public static explicit operator DB.Structure.Rebar(Rebar value) =>
      value?.IsValid == true ? value.Document.GetElement(value) as DB.Structure.Rebar : default;

    public Rebar() { }
    public Rebar(DB.Structure.Rebar rebar) : base(rebar) { }


    public Plane Location
    {
      get
      {
        var rebar = (DB.Structure.Rebar) this;

        if (rebar?.Location is DB.LocationCurve curveLocation)
        {
          var start = curveLocation.Curve.Evaluate(0.0, normalized: true).ToPoint3d();
          var end = curveLocation.Curve.Evaluate(1.0, normalized: true).ToPoint3d();
          var axis = end - start;
          var origin = start + (axis * 0.5);
          var perp = axis.PerpVector();
          return new Plane(origin, axis, perp);
        }

        return this.Location;
      }
    }

    public List<Curve> Curves
    {
      get
      {
        var rebar = (DB.Structure.Rebar) this;

        var curves = new List<Curve>();
        for (int i = 0; i < rebar.NumberOfBarPositions; i++)
        {
          var crvs = rebar.GetCenterlineCurves(false, false, false, DB.Structure.MultiplanarOption.IncludeAllMultiplanarCurves, i).ToArray();
          var transform = rebar.GetShapeDrivenAccessor().GetBarPositionTransform(i);

          foreach (var crv in crvs)
          {
            curves.Add(crv.CreateTransformed(transform).ToCurve());
          }
        }
        return Curve.JoinCurves(curves).ToList();
      }
    }

  }
}
