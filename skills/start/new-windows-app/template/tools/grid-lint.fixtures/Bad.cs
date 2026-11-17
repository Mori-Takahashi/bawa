// Fixture for tools\grid-lint.ps1: 2 lines off the grid. Not compiled.
class Bad
{
    void Layout(dynamic panel, dynamic border)
    {
        border.Margin = new Thickness(10);
        panel.Spacing = 6;
        panel.Spacing = 8;
        var same = panel.ActualWidth == 13;
    }
}
