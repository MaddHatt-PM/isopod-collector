using System;

namespace GraphEditor
{
  [Serializable]
  public class EdgeData : IEquatable<EdgeData>
  {
    public NodeData sourceNode;
    public NodeData destinationNode;

    public float weight;

    public bool Equals(EdgeData other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;

      return sourceNode.Equals(other.sourceNode)
          && destinationNode.Equals(other.destinationNode)
          && weight.Equals(other.weight);
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as EdgeData);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(sourceNode, destinationNode, weight);
    }
  }
}