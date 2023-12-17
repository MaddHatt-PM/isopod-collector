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
    private VisualElement userEventCatcher;
    private VisualElement graphContent;
    private TiledBackground tiledBackground;
    private bool isPanning = false;
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
      root.name = "root-container";
      root.style.flexDirection = FlexDirection.Column;
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

      tiledBackground = new TiledBackground { pickingMode = PickingMode.Ignore };
      tiledBackground.SendToBack();
      root.Add(tiledBackground);

      userEventCatcher = new VisualElement { name = "user-event-catcher" };
      userEventCatcher.AddToClassList("user-event-catcher");
      userEventCatcher.SendToBack();
      root.Add(userEventCatcher);

      // Create a node button
      var addNodeButton = new Button(() => AddNode()) { text = "Add Node" };
      root.Add(addNodeButton);

      // Create the graph content
      graphContent = new VisualElement { name = "graph-content" };
      graphContent.style.flexGrow = 1;
      graphContent.pickingMode = PickingMode.Ignore;
      graphContent.AddToClassList("graph-panel");
      root.Add(graphContent);


      for (int i = 0; i < CurrentGraphData.nodes.Count; i++)
      {
        var nodeData = CurrentGraphData.nodes[i];
        var node = new GraphNode(nodeData);
        node.OnPropertiesChanged += OnNodePropertiesChanged;
        node.OnPrepareNodeForDeletion += OnPrepareNodeForDeletion;
        graphContent.Add(node);
      }


      // Add styles
      var styleSheetPath = "Assets/Toolkit/Editor/GraphEditor/GraphEditorStyles.uss";
      var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
      root.styleSheets.Add(styleSheet);

      // UI Events
      userEventCatcher.AddManipulator(new ContextualMenuManipulator(OnContextMenu));
      userEventCatcher.RegisterCallback<MouseDownEvent>(OnMouseDown);
      userEventCatcher.RegisterCallback<MouseMoveEvent>(OnMouseMove);
      userEventCatcher.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    private void OnContextMenu(ContextualMenuPopulateEvent evt)
    {
      evt.menu.AppendAction(
        actionName: "Create node",
        action: (action) => { AddNode(); },
        actionStatusCallback: DropdownMenuAction.AlwaysEnabled
        );

      evt.menu.AppendSeparator();

      evt.menu.AppendAction(
        actionName: "Reset view",
        action: (action) =>
        {
          panOffset = Vector2.zero;
          graphContent.style.left = panOffset.x;
          graphContent.style.top = panOffset.y;
          tiledBackground.SetOffset(Vector2.zero);
        },
        actionStatusCallback: DropdownMenuAction.AlwaysEnabled
      );
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
      if (evt.button == 0 || evt.button == 2) { isPanning = true; }
      evt.StopPropagation();
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
      if (isPanning)
      {
        panOffset += evt.mouseDelta;
        graphContent.style.left = panOffset.x;
        graphContent.style.top = panOffset.y;
        tiledBackground.SetOffset(panOffset);
        evt.StopPropagation();
      }
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
      isPanning = false;
      evt.StopPropagation();
    }

    void AddNode()
    {
      var nodeData = new NodeData(new Vector2(
            x: graphContent.resolvedStyle.width * 0.5f + panOffset.x,
            y: graphContent.resolvedStyle.height * 0.5f + panOffset.y
          ));
      CurrentGraphData.nodes.Add(nodeData);
      var node = new GraphNode(nodeData);
      node.OnPropertiesChanged += OnNodePropertiesChanged;
      node.OnPrepareNodeForDeletion += OnPrepareNodeForDeletion;
      graphContent.Add(node);
    }

    private void OnNodePropertiesChanged(NodeData nodeData)
    {
      // CurrentGraphData.nodes[nodeIndex] = nodeData;
      var index = CurrentGraphData.nodes.FindIndex(o => o.Uuid == nodeData.Uuid);
      if(index == -1) {
        Debug.LogError("NodeData mismatch from view and model.");
        return;
      }

      CurrentGraphData.nodes[index] = nodeData;
      EditorUtility.SetDirty(CurrentGraphData);
    }

    private void OnPrepareNodeForDeletion(GraphNode node)
    {
      var nodeToDelete = CurrentGraphData.nodes.Find(o => o.Uuid == node.data.Uuid);
      if (nodeToDelete == null)
      {
        Debug.LogError("NodeData mismatch from view and model.");
        return;
      }

      List<EdgeData> newEdges = CurrentGraphData.edges;
      CurrentGraphData.edges.ForEach((edge) =>
      {
        if (edge.sourceNode != nodeToDelete && edge.destinationNode != nodeToDelete)
          newEdges.Add(edge);
      });
      CurrentGraphData.edges = newEdges;

      CurrentGraphData.nodes.Remove(node.data);

      node.OnPropertiesChanged -= OnNodePropertiesChanged;
      node.OnPrepareNodeForDeletion -= OnPrepareNodeForDeletion;
      graphContent.Remove(node);

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
