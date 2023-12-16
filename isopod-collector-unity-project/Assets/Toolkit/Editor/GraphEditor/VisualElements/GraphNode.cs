using UnityEngine;
using UnityEngine.UIElements;

namespace GraphEditor
{
  public class GraphNode : VisualElement
  {
    public readonly NodeData data;
    public readonly int index;

    public delegate void PropertiesChangedDelegate(NodeData data, int index);
    public event PropertiesChangedDelegate OnPropertiesChanged;

    private bool isDragging;
    private Vector2 dragStart;

    private static readonly float translationStepper = 10.0f;
    private static readonly float radius = 44f;

    public GraphNode(NodeData data, int index)
    {
      this.data = data;
      this.index = index;

      // Customize the appearance and behavior of the node
      style.width = 2 * radius;
      style.height = 2 * radius;
      style.borderTopLeftRadius = radius * 0.66f;
      style.borderTopRightRadius = radius * 0.66f;
      style.borderBottomLeftRadius = radius * 0.66f;
      style.borderBottomRightRadius = radius * 0.66f;
      style.position = Position.Absolute;
      style.left = data.editorPosition.x;
      style.top = data.editorPosition.y;
      AddToClassList("graph-node");

      // Add label
      var label = new Label(data.nodeName) {
        name = "Node Title",
      };
      label.style.fontSize = 12;
      Add(label);

      // User Interaction
      RegisterCallback<MouseDownEvent>(OnMouseDown);
      RegisterCallback<MouseUpEvent>(OnMouseUp);
      RegisterCallback<MouseMoveEvent>(OnMouseMove);
    }

    void OnMouseDown(MouseDownEvent evt)
    {
      isDragging = true;
      dragStart = evt.mousePosition;
      BringToFront();
      NotifyPropertiesChange();
      evt.StopPropagation();
    }

    void OnMouseUp(MouseUpEvent evt)
    {
      isDragging = false;
      NotifyPropertiesChange();
      evt.StopPropagation();
    }

    void OnMouseMove(MouseMoveEvent evt)
    {
      if (isDragging)
      {
        var delta = evt.mousePosition - dragStart;
        data.editorPosition += delta;

        style.left = Mathf.Floor(data.editorPosition.x / translationStepper) * translationStepper;
        style.top = Mathf.Floor(data.editorPosition.y / translationStepper) * translationStepper;
        dragStart = evt.mousePosition;

        NotifyPropertiesChange();
        evt.StopPropagation();
      }
    }

    private void NotifyPropertiesChange()
    {
      OnPropertiesChanged?.Invoke(data, index);
    }
  }
}
