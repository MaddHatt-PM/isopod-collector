using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaddHatt.Splines
{

  public class Spline : MonoBehaviour
  {
    [SerializeField] protected Vector3[] points;
    [SerializeField] protected BezierControlPointMode[] modes;
    [SerializeField] protected bool isLooped;

    public int ControlPointCount => points.Length;
    public int CurveCount => (points.Length - 1) / 3;

    public bool IsLooped
    {
      get => isLooped;
      set
      {
        isLooped = value;
        if (value)
        {
          modes[^1] = modes[0];
          SetControlPoint(0, points[0]);
        }
      }
    }

    private void Reset()
    {
      points = new[]
      {
            new Vector3(1, 0, 0),
            new Vector3(2, 0, 0),
            new Vector3(3, 0, 0),
            new Vector3(4, 0, 0),
        };

      modes = new[]
      {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free,
        };
    }

    public Vector3 GetControlPoint(int index)
    {
      return points[index];
    }

    public void SetControlPoint(int index, Vector3 point)
    {
      if (index % 3 == 0)
      {
        var delta = point - points[index];

        if (isLooped)
        {
          if (index == 0)
          {
            points[1] += delta;
            points[^2] += delta;
            points[^1] = point;
          }
          else if (index == points.Length - 1)
          {
            points[0] = point;
            points[1] += delta;
            points[index - 1] += delta;
          }
          else
          {
            points[index - 1] += delta;
            points[index + 1] += delta;
          }
        }
        else
        {
          if (index > 0)
          {
            points[index - 1] += delta;
          }

          if (index + 1 < points.Length)
          {
            points[index + 1] += delta;
          }
        }
      }

      points[index] = point;
      EnforceMode(index);
    }

    public BezierControlPointMode GetControlPointMode(int index)
    {
      return modes[(index + 1) / 3];
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
      var modeIndex = (index + 1) / 3;
      modes[modeIndex] = mode;

      if (isLooped)
      {
        if (modeIndex == 0)
        {
          modes[^1] = mode;
        }
        else if (modeIndex == modes.Length - 1)
        {
          modes[0] = mode;
        }
      }

      EnforceMode(index);
    }

    protected void EnforceMode(int index)
    {
      var modeIndex = (index + 1) / 3;
      var mode = modes[modeIndex];

      if (mode == BezierControlPointMode.Free || !isLooped && (modeIndex == 0 || modeIndex == modes.Length - 1))
      {
        return;
      }

      var middleIndex = modeIndex * 3;
      int fixedIndex;
      int enforcedIndex;

      if (index <= middleIndex)
      {
        fixedIndex = middleIndex - 1;

        if (fixedIndex < 0) fixedIndex = points.Length - 2;
        enforcedIndex = middleIndex + 1;
        if (enforcedIndex >= points.Length) enforcedIndex = 1;
      }

      else
      {
        fixedIndex = middleIndex + 1;
        if (fixedIndex >= points.Length) fixedIndex = 1;

        enforcedIndex = middleIndex - 1;
        if (enforcedIndex < 0) enforcedIndex = points.Length - 2;
      }

      var middle = points[middleIndex];
      var enforcedTangent = middle - points[fixedIndex];

      if (mode == BezierControlPointMode.Aligned)
        enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);

      points[enforcedIndex] = middle + enforcedTangent;
    }

    protected Vector3 GetVelocity(float t)
    {
      int i;

      if (t >= 1f)
      {
        t = 1f;
        i = points.Length - 4;
      }

      else
      {
        t = Mathf.Clamp01(t) * CurveCount;
        i = (int)t;
        t -= i;
        i *= 3;
      }

      var firstDerivative = BezierMath.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t);
      return transform.TransformPoint(firstDerivative) - transform.position;
    }

    public Vector3 GetDirection(float t)
    {
      return GetVelocity(t).normalized;
    }

    public float CalculateArcLength()
    {
      const int d = 10;
      var arcLength = 0f;
      var dt = 1.0f / d;
      Vector3 a = GetPoint(0f);
      Vector3 b;

      for (float t = 0; t <= 1.0f; t += dt)
      {
        b = GetPoint(t);
        arcLength += Vector3.Distance(a, b);
        a = b;
      }

      return arcLength;
    }

    public (Vector3, Vector3)[] CalculatePointsOverTime(int subdivisions = 10)
    {
      var dt = 1.0f / subdivisions;
      var divs = new (Vector3, Vector3)[subdivisions];

      for (int i = 0; i < subdivisions; i++)
        divs[i] = (GetPoint(dt * i), GetDirection(dt * i));

      return divs;
    }

    public (Vector3, Vector3)[] CalculatePointsOverDistance(float distanceThreshold, float resolution = 1)
    {
      var evenlySpacedPoints = new List<(Vector3, Vector3)> { (transform.TransformPoint(points[0]), transform.forward) };
      var prevPoint = points[0];
      var distanceFromLastPoint = 0f;

      for (int i = 0; i < points.Length; i += 4)
      {
        var controlNetLength = Vector3.Distance(points[i], points[i + 1])
          + Vector3.Distance(points[i + 1], points[i + 2])
          + Vector3.Distance(points[i + 2], points[i + 3]);
        var approxArcLength = Vector3.Distance(points[i], points[i + 3]) + controlNetLength / 2f;
        int divisions = Mathf.CeilToInt(approxArcLength * resolution * 10);

        float t = 0;
        while (t <= 1f)
        {
          t += 1f / divisions;
          Vector3 pointOnCurve = BezierMath.EvaluateCubic(points[i], points[i + 1], points[i + 2], points[i + 3], t);
          distanceFromLastPoint += Vector3.Distance(prevPoint, pointOnCurve);

          while (distanceFromLastPoint >= distanceThreshold)
          {
            float overshootDistance = distanceFromLastPoint - distanceThreshold;
            Vector3 evenlySpacedPoint = pointOnCurve + (prevPoint - pointOnCurve).normalized * overshootDistance;
            evenlySpacedPoints.Add((transform.TransformPoint(evenlySpacedPoint), GetDirection(t)));
            distanceFromLastPoint = overshootDistance;
            prevPoint = evenlySpacedPoint;
          }

          prevPoint = pointOnCurve;
        }
      }

      return evenlySpacedPoints.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="segments"></param>
    /// <returns>An array of tuples of positions and directions.</returns>
    public (Vector3, Vector3)[] CalculatePointsIntoSegments(int segments, int precision = 2)
    {
      segments = Mathf.Max(segments - 1, 1);
      var evenlySpacedPoints = new List<(Vector3, Vector3)> { (points[0], transform.forward) };
      var prevPoint = points[0];
      var distanceFromLastPoint = 0f;
      var distanceThreshold = CalculateArcLength() / segments;

      for (int i = 0; i < points.Length; i += 4)
      {
        float t = 0;
        while (t <= 1f)
        {
          t += 1f / (segments * precision);
          Vector3 pointOnCurve = BezierMath.EvaluateCubic(points[i], points[i + 1], points[i + 2], points[i + 3], t);
          distanceFromLastPoint += Vector3.Distance(prevPoint, pointOnCurve);

          while (distanceFromLastPoint >= distanceThreshold)
          {
            float overshootDistance = distanceFromLastPoint - distanceThreshold;
            Vector3 evenlySpacedPoint = pointOnCurve + (prevPoint - pointOnCurve).normalized * overshootDistance;
            evenlySpacedPoints.Add((evenlySpacedPoint, GetDirection(t)));
            distanceFromLastPoint = overshootDistance;
            prevPoint = evenlySpacedPoint;
          }

          prevPoint = pointOnCurve;
        }
      }

      return evenlySpacedPoints.ToArray();
    }


    public Vector3 GetPoint(float t)
    {
      int i;

      if (t >= 1f)
      {
        t = 1f;
        i = points.Length - 4;
      }

      else
      {
        t = Mathf.Clamp01(t) * CurveCount;
        i = (int)t;
        t -= i;
        i *= 3;
      }

      var bezierPoint = BezierMath.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t);
      return transform.TransformPoint(bezierPoint);
    }

    public void AddCurve()
    {
      Array.Resize(ref modes, modes.Length + 1);
      Array.Resize(ref points, points.Length + 3);

      var point = points[^1];
      point.x += 1f;
      points[^3] = point;
      point.x += 1f;
      points[^2] = point;
      point.x += 1f;
      points[^1] = point;

      modes[^1] = modes[^2];
      EnforceMode(points.Length - 4);

      if (isLooped)
      {
        points[^1] = points[0];
        modes[^1] = modes[0];
        EnforceMode(0);
      }
    }
  }
}