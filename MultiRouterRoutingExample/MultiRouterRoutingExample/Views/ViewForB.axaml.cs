using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using MultiRouterRoutingExample.ViewModels;

namespace MultiRouterRoutingExample;

public partial class ViewForB : ReactiveUserControl<TestVMB>
{
    public ViewForB()
    {
        InitializeComponent();
    }
}