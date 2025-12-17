using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;

public class DragAdorner : Adorner
{
    private readonly UIElement _dragVisual;
    private readonly double _offsetX;
    private readonly double _offsetY;

    public DragAdorner(UIElement adornedElement, UIElement dragVisual, double offsetX, double offsetY)
        : base(adornedElement)
    {
        _dragVisual = dragVisual;
        _offsetX = offsetX;
        _offsetY = offsetY;
        AddVisualChild(_dragVisual);
    }

    protected override Visual GetVisualChild(int index)
    {
        return _dragVisual;
    }

    protected override int VisualChildrenCount => 1;

    protected override Size MeasureOverride(Size constraint)
    {
        _dragVisual.Measure(constraint);
        return _dragVisual.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _dragVisual.Arrange(new Rect(_offsetX, _offsetY, _dragVisual.DesiredSize.Width, _dragVisual.DesiredSize.Height));
        return finalSize;
    }
}
