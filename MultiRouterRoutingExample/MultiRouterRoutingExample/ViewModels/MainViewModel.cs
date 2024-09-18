using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace MultiRouterRoutingExample.ViewModels
{
    public class MainViewModel : ViewModelBase, IScreen
    {
        private RoutingState _stackA = new();
        private RoutingState _stackB = new();

        private RoutingState _currentRouter;

        public RoutingState StackA { get { return _stackA; } }
        public RoutingState StackB { get { return _stackB; } }

        public RoutingState Router { get => _currentRouter; set => this.RaiseAndSetIfChanged(ref _currentRouter, value); }

        public ReactiveCommand<Unit, Unit> SwitchToACommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchToBCommand { get; }

        public ReactiveCommand<Unit, Unit> NavigateBothAndToggleCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleAndNavigateBothCommand { get; }

        private ObservableAsPropertyHelper<string> _currentRouterLabel;
        public string CurrentRouterLabel { get => _currentRouterLabel.Value; }

        public MainViewModel()
        {
            _currentRouter = _stackA;
            NavigateBoth();

            SwitchToACommand = ReactiveCommand.Create(() => Dispatcher.UIThread.Post(() => Router = StackA), this.WhenAnyValue((x) => x.Router, selector: r => !r.Equals(StackA)));
            SwitchToBCommand = ReactiveCommand.Create(() => Dispatcher.UIThread.Post(() => Router = StackB), this.WhenAnyValue((x) => x.Router, selector: r => !r.Equals(StackB)));
            NavigateBothAndToggleCommand = ReactiveCommand.Create(() => Dispatcher.UIThread.Post(() => { NavigateBoth(); Toggle(); }));
            ToggleAndNavigateBothCommand = ReactiveCommand.Create(() => Dispatcher.UIThread.Post(() => { Toggle(); NavigateBoth(); }));

            _currentRouterLabel = this.WhenAnyValue((x) => x.Router)
                .Select(nr => nr.Equals(StackA) ? "Stack A" : "Stack B")
                .ToProperty(this, (x) => x.CurrentRouterLabel);
        }

        private void NavigateBoth()
        {
            if (Router.Equals(StackA))
            {
                StackA.Navigate.Execute(new TestVMA(this, Random.Shared.NextInt64())).Subscribe();
                StackB.Navigate.Execute(new TestVMB(this, Random.Shared.NextInt64())).Subscribe();
            }
            else
            {
                StackB.Navigate.Execute(new TestVMB(this, Random.Shared.NextInt64())).Subscribe();
                StackA.Navigate.Execute(new TestVMA(this, Random.Shared.NextInt64())).Subscribe();
            }
        }
        private void Toggle()
        {
            Router = Router.Equals(StackA) ? StackB : StackA;
        }

    }

    public class TestVMA : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment { get => "Type A"; }

        private IScreen _screen;
        public IScreen HostScreen { get => _screen; }

        public long ID { get; init; }

        public TestVMA(IScreen screen, long id) 
        {
            _screen = screen;
            ID = id;
        }

    }

    public class TestVMB : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment { get => "Type B"; }

        private IScreen _screen;
        public IScreen HostScreen { get => _screen; }

        public long ID { get; init; }

        public TestVMB(IScreen screen, long id)
        {
            _screen = screen;
            ID = id;
        }

    }


}
