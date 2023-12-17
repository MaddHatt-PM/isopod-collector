using UnityEngine;
using UnityEngine.UIElements;

namespace GraphEditor
{
  public class GraphNode : VisualElement
  {
    public readonly NodeData data;

    public delegate void PropertiesChangedDelegate(NodeData data);
    public event PropertiesChangedDelegate OnPropertiesChanged;

    public delegate void PrepareNodeForDeletion(GraphNode graphNode);
    public event PrepareNodeForDeletion OnPrepareNodeForDeletion;

    public delegate void ReleaseMouseEventsDelegate();
    public static event ReleaseMouseEventsDelegate OnReleaseMouseEvents;

    private bool isDragging;
    private Vector2 dragStart;

    private static readonly float translationStepper = 10.0f;
    private static readonly float radius = 50f;
    private static readonly float cornerPercentage = 0.666f;

    public GraphNode(NodeData data)
    {
      this.data = data;

      // Customize the appearance and behavior of the node
      style.width = 2 * radius;
      style.height = 2 * radius;
      style.borderTopLeftRadius = radius * cornerPercentage;
      style.borderTopRightRadius = radius * cornerPercentage;
      style.borderBottomLeftRadius = radius * cornerPercentage;
      style.borderBottomRightRadius = radius * cornerPercentage;
      style.position = Position.Absolute;
      style.left = data.editorPosition.x;
      style.top = data.editorPosition.y;
      AddToClassList("graph-node");

      // Add label
      var titleLabel = new Label(data.nodeName)
      {
        name = "Node Title",
      };
      titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
      titleLabel.style.fontSize = 14;
      Add(titleLabel);

      // User Interaction
      this.AddManipulator(new ContextualMenuManipulator(OnContextMenu));
      RegisterCallback<MouseDownEvent>(OnMouseDown);
      RegisterCallback<MouseUpEvent>(OnMouseUp);
      RegisterCallback<MouseMoveEvent>(OnMouseMove);

      OnReleaseMouseEvents += ReleaseMouseEventsForOthers;
    }

    private void OnContextMenu(ContextualMenuPopulateEvent evt) {
      evt.menu.AppendAction(
        actionName: "Delete node and all connections",
        action: (action) => {
          OnPrepareNodeForDeletion?.Invoke(this);
          },
        actionStatusCallback: DropdownMenuAction.AlwaysEnabled
      );
    }

    void OnMouseDown(MouseDownEvent evt)
    {

      if (evt.button == 0)
      {
        OnReleaseMouseEvents(); ;
        isDragging = true;
        dragStart = evt.mousePosition;
        BringToFront();
        NotifyPropertiesChange();
        evt.StopPropagation();
      }
    }

    void OnMouseUp(MouseUpEvent evt)
    {
      isDragging = false;
      style.left = Mathf.RoundToInt(data.editorPosition.x / translationStepper) * translationStepper;
      style.top = Mathf.RoundToInt(data.editorPosition.y / translationStepper) * translationStepper;
      NotifyPropertiesChange();
      evt.StopPropagation();
    }

    void OnMouseMove(MouseMoveEvent evt)
    {
      if (isDragging)
      {
        var delta = evt.mousePosition - dragStart;
        data.editorPosition += delta;

        style.left = data.editorPosition.x;
        style.top = data.editorPosition.y;
        dragStart = evt.mousePosition;

        NotifyPropertiesChange();
        evt.StopPropagation();
      }
    }

    private void ReleaseMouseEventsForOthers()
    {
      isDragging = false;
    }

    private void NotifyPropertiesChange()
    {
      OnPropertiesChanged?.Invoke(data);
    }
  }
}
