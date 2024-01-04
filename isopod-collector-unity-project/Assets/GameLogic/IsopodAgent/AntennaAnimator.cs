using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using System;


public class AntennaAnimator : MonoBehaviour
{
  [Header("Animation Variables")]
  [SerializeField] Vector2 horAngleExtents;
  [SerializeField] Vector2 verAngleExtents;
  [SerializeField, Range(0f, 1f)] float hierarchyDampener;
  [SerializeField] float initialVelocity;
  [SerializeField, Min(1f)] float speedMultiplier = 1f;

  int maxDepth = 0;
  List<SegmentData> segmentData;

  private class SegmentData
  {
    [ShowInInspector] public int depth;
    public Transform transform;
    public Quaternion initialRotation;

    Vector2 horAngleExtents;
    Vector2 verAngleExtents;
    Quaternion prevRotation;
    Quaternion nextRotation;

    public float currTime;
    public float targetTime;
    public float angularVelocity;

    public SegmentData(int depth, Transform transform)
    {
      this.depth = depth;
      this.transform = transform;
      initialRotation = transform.localRotation;
    }

    public void PostInitialize(float velocity, Vector2 horAngleExtents, Vector2 verAngleExtents)
    {
      var length = transform.gameObject.GetComponent<Renderer>()?.bounds.size.z ?? 1f;
      angularVelocity = velocity * length;
      this.horAngleExtents = horAngleExtents;
      this.verAngleExtents = verAngleExtents;
    }

    public void SetInitialRotation()
    {
      var horValue = Mathf.Lerp(horAngleExtents.x, horAngleExtents.y, UnityEngine.Random.value);
      var verValue = Mathf.Lerp(verAngleExtents.x, verAngleExtents.y, UnityEngine.Random.value);
      prevRotation = Quaternion.Euler(0f, horValue, 0f) * Quaternion.Euler(0f, 0f, verValue) * initialRotation;

      transform.localRotation = prevRotation;
      SetNewTarget();
    }

    public void SetNewTarget()
    {
      targetTime = -1f;
      while (targetTime < 0.01f)
      {
        prevRotation = transform.localRotation;
        var horValue = Mathf.Lerp(horAngleExtents.x, horAngleExtents.y, UnityEngine.Random.value);
        var verValue = Mathf.Lerp(verAngleExtents.x, verAngleExtents.y, UnityEngine.Random.value);
        nextRotation = Quaternion.Euler(0f, horValue, 0f) * Quaternion.Euler(0f, 0f, verValue) * initialRotation;
        currTime = 0f;
        targetTime = Quaternion.Angle(prevRotation, nextRotation) / angularVelocity;
      }
    }

    public void TickAnimation(float speedMultiplier)
    {
      currTime += Time.deltaTime * speedMultiplier;
      if (currTime >= targetTime) SetNewTarget();

      var localRotation = Quaternion.Lerp(prevRotation, nextRotation, currTime / targetTime);
      transform.localRotation = localRotation;
    }
  }

  void Start()
  {
    segmentData = new();
    var queue = new Queue<(int, Transform)>();
    queue.Enqueue((1, transform));

    while (queue.Count != 0)
    {
      var (depth, target) = queue.Dequeue();
      segmentData.Add(new SegmentData(depth, target));
      maxDepth = Math.Max(depth, maxDepth);

      foreach (Transform child in target)
        queue.Enqueue((depth + 1, child));
    }

    Debug.Log(maxDepth);

    foreach (var seg in segmentData)
    {
      var velocityDampener = hierarchyDampener * (seg.depth / (maxDepth + 1f));
      var extentDampener = Mathf.Pow(hierarchyDampener, maxDepth - seg.depth);
      seg.PostInitialize(initialVelocity * velocityDampener, horAngleExtents * extentDampener, verAngleExtents * extentDampener);
      seg.SetInitialRotation();
    }
  }

  [ContextMenu("Test rotation")]
  void Test()
  {
    foreach (var seg in segmentData)
      seg.SetInitialRotation();
  }

  void Update()
  {
    foreach (var seg in segmentData)
      seg.TickAnimation(speedMultiplier);
  }
}
