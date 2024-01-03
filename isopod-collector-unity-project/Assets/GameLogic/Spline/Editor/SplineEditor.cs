using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MaddHatt.Splines
{


  /// <summary>
  /// Custom inspector to control spline points interactively in the scene view.
  /// </summary>
  [CustomEditor(typeof(Spline))]
  public class SplineEditor : Editor
  {
    protected const float handleSize = 0.04f;
    protected const float pickSize = 0.06f;

    protected static readonly Dictionary<BezierControlPointMode, Color> modeColors = new()
    {
        { BezierControlPointMode.Aligned, Color.yellow },
        { BezierControlPointMode.Free, Color.white },
        { BezierControlPointMode.Mirrored, Color.cyan },
    };

    protected Spline spline;
    protected Transform handleTransform;
    protected Quaternion handleRotation;
    protected int selectedIndex = -1;

    public override void OnInspectorGUI()
    {
      spline = target as Spline;

      if (spline == null) return;

      using (var check = new EditorGUI.ChangeCheckScope())
      {
        var isLooped = EditorGUILayout.Toggle("Loop", spline.IsLooped);

        if (check.changed)
        {
          Undo.RecordObject(spline, "Toggle Loop");
          EditorUtility.SetDirty(spline);
          spline.IsLooped = isLooped;
        }
      }

      var isPointSelected = selectedIndex >= 0 && selectedIndex < spline.ControlPointCount;
      if (isPointSelected) DrawSelectedPointInspector();

      if (GUILayout.Button("Add Curve"))
      {
        Undo.RecordObject(spline, "Add Curve");
        EditorUtility.SetDirty(spline);
        spline.AddCurve();
      }
    }

    public virtual void OnSceneGUI()
    {
      spline = target as Spline;

      if (spline == null) return; 

      handleTransform = spline.transform;

      handleRotation = Tools.pivotRotation == PivotRotation.Local
          ? handleTransform.rotation
          : Quaternion.identity;

      var point0 = ShowPoint(0);

      for (var i = 1; i < spline.ControlPointCount; i += 3)
      {
        var point1 = ShowPoint(i);
        var point2 = ShowPoint(i + 1);
        var point3 = ShowPoint(i + 2);

        Handles.color = Color.white;
        Handles.DrawLine(point0, point1);
        Handles.DrawLine(point2, point3);

        Handles.DrawBezier(point0, point3, point1, point2, Color.white, null, 4f);
        point0 = point3;
      }
    }

    protected void DrawSelectedPointInspector()
    {
      using (var check = new EditorGUI.ChangeCheckScope())
      {
        var point = EditorGUILayout.Vector3Field("Selected Point", spline.GetControlPoint(selectedIndex));

        if (check.changed)
        {
          Undo.RecordObject(spline, "Move Point");
          EditorUtility.SetDirty(spline);
          spline.SetControlPoint(selectedIndex, point);
        }
      }

      using (var check = new EditorGUI.ChangeCheckScope())
      using (new EditorGUILayout.HorizontalScope())
      {
        EditorGUILayout.PrefixLabel(" ");
        var mode = (BezierControlPointMode)EditorGUILayout.EnumPopup(spline.GetControlPointMode(selectedIndex));

        if (check.changed)
        {
          Undo.RecordObject(spline, "Change Point Mode");
          EditorUtility.SetDirty(spline);
          spline.SetControlPointMode(selectedIndex, mode);
        }
      }
    }

    protected virtual Vector3 ShowPoint(int index, bool showPoint = true)
    {
      var DrawPoint = handleTransform.TransformPoint(spline.GetControlPoint(index));
      var size = HandleUtility.GetHandleSize(DrawPoint);

      if (index == 0) size *= 2;

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
        }
      }

      return DrawPoint;
    }
  }
}