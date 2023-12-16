using System.Collections.Generic;
using UnityEngine;

namespace GraphEditor
{
  [CreateAssetMenu(fileName = "GraphData", menuName = "Tools/new Graph Data", order = 1)]
  public class GraphData : ScriptableObject
  {
    public List<NodeData> nodes = new List<NodeData>();
    public List<EdgeData> edges = new List<EdgeData>();
  }
}