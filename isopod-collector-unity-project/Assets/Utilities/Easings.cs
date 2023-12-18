using UnityEngine;

public class Easings
{
  public static float EaseOutCubic(float t)
  {
    return 1 - Mathf.Pow(1 - t, 3);
  }

  public static float EaseOutCubic(float a, float b, float t)
  {
    return Mathf.Lerp(a, b, EaseOutCubic(t));
  }

  public static float EaseOutCircular(float t)
  {
    return Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
  }

  public static float EaseOutCircular(float a, float b, float t)
  {
    return Mathf.Lerp(a, b, EaseOutCircular(t));
  }
}