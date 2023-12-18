using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class IsopodMover : MonoBehaviour
{
  [FoldoutGroup("References"), SerializeField] private Transform headJointRef;
  // [FoldoutGroup("References"), SerializeField] private Transform torsoJointRef;
  [FoldoutGroup("References"), SerializeField] private Transform tailJointRef;
  [FoldoutGroup("References"), SerializeField] private Transform headMeshRef;
  // [FoldoutGroup("References"), SerializeField] private Transform torsoMeshRef;
  [FoldoutGroup("References"), SerializeField] private Transform tailMeshRef;

  [SerializeField] private GameObject segmentPrefab;
  [SerializeField] private List<Transform> torsoMeshSegments;

  [SerializeField] private bool executeInEditMode = false;

  [SerializeField] private float headToTailLength = 1f;
  [SerializeField, Range(0.1f, 1f)] private float torsoDistanceThreshold = 0.125f;

  [SerializeField] private float maxAngleOfTurning = 30f;
  private float maxRadianOfTurning { get => Mathf.Deg2Rad * maxAngleOfTurning; }
  [SerializeField, Range(-1f, 1f)] private float normTurningAngle = 0.0f;

  [SerializeField] private AnimationCurve meshScalingCurve = AnimationCurve.Linear(0f, 0f, 0f, 1f);
  [Range(0f, 1f)] private float startScale = 1.0f;
  [SerializeField] private float finalScale = 1.0f;

  [SerializeField] private AnimationCurve meshSquashCurve;
  [SerializeField, Range(0f, 1f)] private float meshSquashMin = 0.5f;

  void Start()
  {
    InitMeshModification();
  }

  void Update()
  {
    if (Application.isPlaying)
    {

    }
    else if (executeInEditMode)
    {
      InitMeshModification();
      ProcessLocomotion();
    }
  }

  private void InitMeshModification()
  {
    if (new List<Transform>() { headJointRef, tailJointRef, headMeshRef, tailMeshRef }.Any(o => o == null))
    {
      Debug.LogError("A reference has not been set.");
      return;
    }

    headJointRef.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    tailJointRef.SetLocalPositionAndRotation(new Vector3(-1.0f * headToTailLength, 0f, 0f), Quaternion.identity);

    // headMeshRef.SetLocalPositionAndRotation(headJointRef.localPosition, headJointRef.localRotation);
    // tailMeshRef.SetLocalPositionAndRotation(tailJointRef.localPosition, tailJointRef.localRotation);

    // headMeshRef.localScale = Vector3.one * Mathf.Lerp(startScale, finalScale, meshScalingCurve.Evaluate(0f));
    // tailMeshRef.localScale = Vector3.one * Mathf.Lerp(startScale, finalScale, meshScalingCurve.Evaluate(1f));

    foreach (var segment in torsoMeshSegments)
      DestroyImmediate(segment.gameObject);
    torsoMeshSegments.Clear();

    int torsoMeshCount = (int)(headToTailLength / torsoDistanceThreshold);
    torsoMeshCount = Mathf.Max(torsoMeshCount, 3);
    float torsoMeshDistance = headToTailLength * torsoDistanceThreshold;
    var parent = headMeshRef.parent;
    for (int i = 0; i < torsoMeshCount; i++)
    {
      var segment = Instantiate(original: segmentPrefab).transform;
      segment.name = "Segment [Auto]";
      segment.SetParent(parent, false);
      segment.SetLocalPositionAndRotation(
        localPosition: new Vector3(torsoMeshDistance * i + tailJointRef.localPosition.x, 0f, 0f),
        localRotation: Quaternion.identity
      );
      segment.localScale = new Vector3(1f, Mathf.Lerp(meshSquashMin, 1f, meshSquashCurve.Evaluate((float)i / torsoMeshCount)), 1f) * Mathf.Lerp(startScale, finalScale, meshScalingCurve.Evaluate(Vector3.Distance(headJointRef.localPosition, segment.localPosition) / headToTailLength));
      torsoMeshSegments.Add(segment);
    }
  }

  private void ProcessLocomotion()
  {
    var turningAngle = normTurningAngle * maxAngleOfTurning;
    headJointRef.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    headJointRef.RotateAround(tailJointRef.position, tailJointRef.up, turningAngle * ((torsoMeshSegments.Count + 1f) / torsoMeshSegments.Count));
    headJointRef.localRotation = Quaternion.Euler(new Vector3(0f, turningAngle * 1.75f, 0f));

    int torsoMeshCount = (int)(headToTailLength / torsoDistanceThreshold);
    float torsoMeshDistance = headToTailLength / torsoMeshCount;

    for (int i = 0; i < torsoMeshSegments.Count; i++)
    {
      var segment = torsoMeshSegments[i];
      segment.SetLocalPositionAndRotation(
        localPosition: new Vector3((float)(torsoMeshDistance * i) * headToTailLength + tailJointRef.localPosition.x, 0f, 0f),
        localRotation: Quaternion.identity);

      segment.RotateAround(
        point: tailJointRef.position,
        axis: tailJointRef.up,
        angle: (float)i / torsoMeshSegments.Count * turningAngle);

      segment.localRotation = Quaternion.Euler(new Vector3(
        x: 0f,
        y: Easings.EaseOutCubic((float)i / torsoMeshSegments.Count) * turningAngle * Easings.EaseOutCubic(1f, 1.5f, (float)i / torsoMeshSegments.Count),
        z: 0f));
    }

    headMeshRef.SetLocalPositionAndRotation(torsoMeshSegments.Last().localPosition, torsoMeshSegments.Last().localRotation);
    tailMeshRef.SetLocalPositionAndRotation(tailJointRef.localPosition, tailJointRef.localRotation);
  }

}
