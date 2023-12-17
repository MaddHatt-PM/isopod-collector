using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GraphEditor
{
  [Serializable]
  public class NodeData : IEquatable<NodeData>
  {
    public string nodeName;
    public string nodeInfo;
    [SerializeField, ReadOnly] private string _uuid;
    public Guid Uuid
    {
      get
      {
        _uuid ??= Guid.NewGuid().ToString();
        return new Guid(_uuid);
      }
    }

    public Vector2 editorPosition;

    public NodeData()
    {
      nodeName = "name";
      _uuid = Guid.NewGuid().ToString();
      editorPosition = new Vector2(100, 100);
    }

    public NodeData(Vector2 position)
    {
      nodeName = "name";
      _uuid = Guid.NewGuid().ToString();
      editorPosition = position;
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