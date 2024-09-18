using Avalonia.Controls;
using MultiRouterRoutingExample.ViewModels;
using ReactiveUI;
using System;

namespace MultiRouterRoutingExample.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }
    }

    public class TestLocator : IViewLocator
    {
        public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
        {
            return viewModel switch
            {
                TestVMA vma => new ViewForA() { DataContext = vma },
                TestVMB vmb => new ViewForB() { DataContext = vmb },
                _ => throw new NotImplementedException()
            };
        }
    }
}