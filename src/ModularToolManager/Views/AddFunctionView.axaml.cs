using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ModularToolManager.Views;

public partial class AddFunctionView : UserControl
{
    public AddFunctionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
