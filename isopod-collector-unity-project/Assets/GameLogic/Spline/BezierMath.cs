using UnityEngine;

namespace MaddHatt.Splines
{
  public static class BezierMath
  {
    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
      t = Mathf.Clamp01(t);
      var oneMinusT = 1f - t;

      return
        3f * oneMinusT * oneMinusT * (p1 - p0)
        + 6f * oneMinusT * t * (p2 - p1)
        + 3f * t * t * (p3 - p2);
    }

    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
      t = Mathf.Clamp01(t);
      var oneMinusT = 1f - t;

      return
        oneMinusT * oneMinusT * oneMinusT * p0
        + 3f * oneMinusT * oneMinusT * t * p1
        + 3f * oneMinusT * t * t * p2
        + t * t * t * p3;
    }

    public static Vector3 EvaluateQuadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
      var a = Vector3.Lerp(p0, p1, t);
      var b = Vector3.Lerp(p1, p2, t);
      return Vector3.Lerp(a, b, t);
    }

    public static Vector3 EvaluateCubic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
      var a = EvaluateQuadratic(p0, p1, p2, t);
      var b = EvaluateQuadratic(p1, p2, p3, t);
      return Vector3.Lerp(a, b, t);
    }
  }
}