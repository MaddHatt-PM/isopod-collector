using System.Runtime.InteropServices;
using MaddHatt.Splines;
using UnityEditor;
using UnityEngine;

namespace MaddHatt.Isopods
{
  [CustomEditor(typeof(IsopodSpine))]
  public class IsopodSpineUnconstrainedEditor : SplineEditor
  {
    public override void OnInspectorGUI()
    {
      spline = target as Spline;
      if (spline == null) return;

      EditorGUILayout.LabelField($"ArcLength: {spline.CalculateArcLength()}");
      // base.OnInspectorGUI();
      DrawDefaultInspector();
    }

    public override void OnSceneGUI()
    {
      spline = target as Spline;
      var isopodSpine = target as IsopodSpine;
      if (spline == null) return;

      handleTransform = spline.transform;

      handleRotation = Tools.pivotRotation == PivotRotation.Local
          ? handleTransform.rotation
          : Quaternion.identity;

      var point0 = ShowPoint(0);

      for (var i = 1; i < spline.ControlPointCount; i += 3)
      {
        var point1 = ShowPoint(i);
        var point2 = ShowPoint(i + 1, false);
        var point3 = ShowPoint(i + 2);

        Handles.color = Color.white;
        Handles.DrawLine(point0, point1);
        Handles.DrawLine(point2, point3);

        Handles.DrawBezier(point0, point2, point1, point2, Color.white, null, 4f);
        point0 = point3;


        foreach (var (p, r) in spline.CalculatePointsIntoSegments(isopodSpine.segments))
        {
          var localP = spline.transform.TransformPoint(p);
          var size = HandleUtility.GetHandleSize(handleTransform.TransformPoint(p));
          var prevColor = Handles.color;
          Handles.color = Color.blue;
          Handles.DrawAAPolyLine(8f, new Color[] { Color.yellow, Color.yellow }, new Vector3[] { localP, localP + r * 0.1f });
          Handles.DrawWireCube(localP, pickSize * Vector3.one);
          Handles.color = prevColor;
        }
      }
    }

    protected override Vector3 ShowPoint(int index, bool showPoint = true)
    {
      var DrawPoint = handleTransform.TransformPoint(spline.GetControlPoint(index));
      var size = HandleUtility.GetHandleSize(DrawPoint);

      if (index == 0)
        size *= 2;

      var controlPointMode = spline.GetControlPointMode(index);
      Handles.color = modeColors[controlPointMode];

      if (showPoint && Handles.Button(DrawPoint, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
      {
        selectedIndex = index;
        Repaint();
      }

      if (selectedIndex == index)
      {
        using var check = new EditorGUI.ChangeCheckScope();
        DrawPoint = Handles.DoPositionHandle(DrawPoint, handleRotation);

        if (check.changed)
        {
          Undo.RecordObject(spline, "Move Point");
          EditorUtility.SetDirty(spline);
          spline.SetControlPoint(index, handleTransform.InverseTransformPoint(DrawPoint));
          if (index == spline.ControlPointCount - 1)
            spline.SetControlPoint(index - 1, handleTransform.InverseTransformPoint(DrawPoint));
        }
      }

      return DrawPoint;
    }
  }
}