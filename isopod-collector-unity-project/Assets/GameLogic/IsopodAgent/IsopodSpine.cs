using MaddHatt.Splines;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[ExecuteInEditMode]
public class IsopodSpine : Spline
{
  [SerializeField, HideInInspector] Transform scalerTransform;

  [Header("Template Resources")]
  public GameObject tailTemplate;
  public GameObject torsoTemplate;
  public GameObject headTemplate;

  [Header("Creation Values")]
  public int segments = 7;
  public float arcLength = 1f;
  public float lifeStageScaler = 1f;
  public float individualSizeScaler = 1f;

  public Vector2 widthTaperExtents = Vector2.one;
  [LabelText("Width Taper (tail=0)")] public AnimationCurve widthTaper;

  public Vector2 heightTaperExtents = Vector2.one;
  [LabelText("Height Taper (tail=0)")] public AnimationCurve heightTaper;

  public Vector2 lengthTaperExtents = Vector2.one;
  [LabelText("Length Taper (tail=0)")] public AnimationCurve lengthTaper;

  [Header("Animatable Values")]
  [SerializeField] Vector2 horizontalExtents = new(-90f, 90f);
  [SerializeField, Range(-1f, 1f)] float horizontalAim;
  [SerializeField] Vector2 verticalExtents = new(-90f, 45f);
  [SerializeField, Range(-1f, 1f)] float verticalAim;

  [SerializeField, Range(0f, 1f)] float archValue = 0f;
  [SerializeField] float archRatioToLength = 1f;

  [SerializeField, HideInInspector] List<Transform> segmentTransforms;

  Vector3 origTailPoint;
  Vector3 origMidPoint;
  Vector3 origHeadPoint;

  [ContextMenu("Reset Spine")]
  public void ResetSpine()
  {
    origTailPoint = new Vector3(0, 0, 0);
    origMidPoint = new Vector3(0, 0, arcLength * 0.5f);
    origHeadPoint = new Vector3(0, 0, arcLength);

    points = new[] {
        origTailPoint,
        origMidPoint,
        origHeadPoint, 
        origHeadPoint // Intentional duplicate for quadratic bezier
        };

    modes = new[] {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free };

    segmentTransforms ??= new();

    foreach (Transform child in transform)
    {
      if (Application.isPlaying)
        Destroy(child.gameObject);
      else
        DestroyImmediate(child.gameObject);
    }

    scalerTransform = new GameObject("Scaler").transform;
    scalerTransform.localScale = lifeStageScaler * individualSizeScaler * Vector3.one;
    scalerTransform.SetParent(transform, worldPositionStays: false);

    segmentTransforms.Clear();

    var tail = Instantiate(tailTemplate).transform;
    tail.SetParent(scalerTransform, worldPositionStays: false);
    tail.localScale = new Vector3(
      x: Mathf.Lerp(widthTaperExtents.x, widthTaperExtents.y, widthTaper.Evaluate(0f)),
      y: Mathf.Lerp(heightTaperExtents.x, heightTaperExtents.y, heightTaper.Evaluate(0f)),
      z: Mathf.Lerp(lengthTaperExtents.x, lengthTaperExtents.y, lengthTaper.Evaluate(0f)));
    segmentTransforms.Add(tail);

    for (int i = 1; i < segments - 1; i++)
    {
      var torso = Instantiate(torsoTemplate).transform;
      torso.transform.SetParent(scalerTransform, worldPositionStays: false);
      torso.localScale = new Vector3(
        x: Mathf.Lerp(widthTaperExtents.x, widthTaperExtents.y, widthTaper.Evaluate((float)i / segments)),
        y: Mathf.Lerp(heightTaperExtents.x, heightTaperExtents.y, heightTaper.Evaluate((float)i / segments)),
        z: Mathf.Lerp(lengthTaperExtents.x, lengthTaperExtents.y, lengthTaper.Evaluate((float)i / segments)));
      segmentTransforms.Add(torso);
    }

    var head = Instantiate(headTemplate).transform;
    head.transform.SetParent(scalerTransform, worldPositionStays: false);
    head.localScale = new Vector3(
        x: Mathf.Lerp(widthTaperExtents.x, widthTaperExtents.y, widthTaper.Evaluate(((float)segments) / segments)),
        y: Mathf.Lerp(heightTaperExtents.x, heightTaperExtents.y, heightTaper.Evaluate(((float)segments) / segments)),
        z: Mathf.Lerp(lengthTaperExtents.x, lengthTaperExtents.y, lengthTaper.Evaluate(((float)segments) / segments)));
    segmentTransforms.Add(head);
    UpdateSegmentTransforms();
  }

  private void Update()
  {
    var mid = origMidPoint;
    Vector3 rotationOrigin = new Vector3(0, 0, arcLength) - mid;

    var horAngle = Mathf.Lerp(horizontalExtents.x, horizontalExtents.y, (horizontalAim * 0.5f) + 0.5f);
    var reducer = Mathf.Sin(1f - Mathf.Abs(horizontalAim));
    var verAngle = verticalAim < 0f
          ? Mathf.Lerp(0f, -1f * verticalExtents.y, Mathf.Abs(verticalAim) * reducer)
          : Mathf.Lerp(0f, -1f * verticalExtents.x, verticalAim * reducer);

    Vector3 p = Quaternion.Euler(verAngle, horAngle, 0f) * rotationOrigin / CalculateArcLength();
    p += mid;

    var arch = arcLength * archRatioToLength * archValue;
    mid.y += arch;
    points[1] = mid;

    points[2] = p;
    points[3] = p;

    UpdateSegmentTransforms();
  }

  private void UpdateSegmentTransforms()
  {
    if (segmentTransforms == null || segmentTransforms.Count == 0) return;
    int i = 0;
    var transformData = CalculatePointsIntoSegments(segments, precision: 3);
    foreach (var (pos, rot) in transformData)
    {
      segmentTransforms[i].transform.localPosition = pos;
      segmentTransforms[i].transform.rotation = Quaternion.LookRotation(rot, transform.up);
      i++;
    }

    var blendedTailRotation = Vector3.Lerp(transformData[0].Item2, transformData[1].Item2, 0.666f);
    segmentTransforms[0].transform.rotation = Quaternion.LookRotation(blendedTailRotation, transform.up);
  }
}