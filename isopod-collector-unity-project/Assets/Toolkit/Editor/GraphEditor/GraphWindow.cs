using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using UnityEditor.UIElements;
// using System;

namespace GraphEditor
{
  public class GraphWindow : EditorWindow
  {
    [SerializeField] private GraphData _currentGraphData = null;
    public GraphData CurrentGraphData
    {
      get { return _currentGraphData; }
      private set
      {
        _currentGraphData = value;
        root.Clear();
        CreateUI();
      }
    }

    private VisualElement root;
    private VisualElement graphContent;
    private Vector2 panOffset = Vector2.zero;


    [MenuItem("Tools/Graph Editor")]
    public static GraphWindow ShowWindow()
    {
      return GetWindow<GraphWindow>("Graph Editor");
    }

    void OnEnable()
    {
      CreateUI();
    }

    public void SetCurrentGraphData(GraphData graphData)
    {
      CurrentGraphData = graphData;
    }

    void CreateUI()
    {
      root = rootVisualElement;
      var label = new Label("Select a graph data");

      if (CurrentGraphData == null)
      {
        var objectField = new ObjectField("Select a ScriptableObject") { objectType = typeof(GraphData) };
        root.Add(label);

        objectField.RegisterValueChangedCallback(evt =>
        {
          CurrentGraphData = evt.newValue as GraphData;
          root.Clear();
          CreateUI();
        });
        root.Add(objectField);
        return;
      }

      var toolbarPanel = new VisualElement();
      var statusBar = new VisualElement();

      // Create a node button
      var addNodeButton = new Button(() => AddNode()) { text = "Add Node" };
      root.Add(addNodeButton);

      // Create the graph content
      graphContent = new VisualElement { name = "MainPanel" };
      graphContent.AddToClassList("graph-panel");
      root.Add(graphContent);

      for (int i = 0; i < CurrentGraphData.nodes.Count; i++)
      {
        var nodeData = CurrentGraphData.nodes[i];
        var node = new GraphNode(nodeData, i);
        node.OnPropertiesChanged += OnNodePropertiesChanged;
        graphContent.Add(node);
      }

      // Add styles
      root.style.flexDirection = FlexDirection.Column;
      graphContent.style.flexGrow = 1;
      var styleSheetPath = "Assets/Toolkit/Editor/GraphEditor/GraphEditorStyles.uss";
      var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
      root.styleSheets.Add(styleSheet);
    }

    void AddNode()
    {
      var nodeData = new NodeData(position: new Vector2(
            x: graphContent.resolvedStyle.width * 0.5f + panOffset.x,
            y: graphContent.resolvedStyle.height * 0.5f + panOffset.y
          ));
      CurrentGraphData.nodes.Add(nodeData);
      var nodeVisualElement = new GraphNode(nodeData, CurrentGraphData.nodes.Count - 1);
      graphContent.Add(nodeVisualElement);
    }

    private void OnNodePropertiesChanged(NodeData nodeData, int nodeIndex)
    {
      // Debug.Log($"Node properties changed for {nodeData.nodeName ?? "INVALID"}");
      CurrentGraphData.nodes[nodeIndex] = nodeData;
      EditorUtility.SetDirty(CurrentGraphData);
    }

    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
      Object obj = EditorUtility.InstanceIDToObject(instanceID);
      if (obj is GraphData)
      {
        var window = ShowWindow();
        window.CurrentGraphData = obj as GraphData;
        return true;
      }
      return false;
    }
  }
}
