using System;
using UnityEngine;

namespace GraphEditor
{
  [Serializable]
  public class NodeData : IEquatable<NodeData>
  {
    public string nodeName;
    public string nodeInfo;

    public Vector2 editorPosition;

    public NodeData(Vector2 position) {
      nodeName = "name";
      editorPosition = new Vector2(100,100);
    }

    public bool Equals(NodeData other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;

      return nodeName == other.nodeName
          && nodeInfo == other.nodeInfo
          && editorPosition.Equals(other.editorPosition);
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as NodeData);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(nodeName, nodeInfo, editorPosition);
    }
  }
}