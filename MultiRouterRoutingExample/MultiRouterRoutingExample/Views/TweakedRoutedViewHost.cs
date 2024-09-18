using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace MultiRouterRoutingExample.Views
{
    public class TweakedRoutedViewHost : TransitioningContentControl, IActivatableView, IEnableLogger
    {
        /// <summary>
        /// <see cref="AvaloniaProperty"/> for the <see cref="Router"/> property.
        /// </summary>
        public static readonly StyledProperty<RoutingState?> RouterProperty =
            AvaloniaProperty.Register<RoutedViewHost, RoutingState?>(nameof(Router));

        /// <summary>
        /// <see cref="AvaloniaProperty"/> for the <see cref="ViewContract"/> property.
        /// </summary>
        public static readonly StyledProperty<string?> ViewContractProperty =
            AvaloniaProperty.Register<RoutedViewHost, string?>(nameof(ViewContract));

        /// <summary>
        /// <see cref="AvaloniaProperty"/> for the <see cref="DefaultContent"/> property.
        /// </summary>
        public static readonly StyledProperty<object?> DefaultContentProperty =
            ViewModelViewHost.DefaultContentProperty.AddOwner<RoutedViewHost>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedViewHost"/> class.
        /// </summary>
        public TweakedRoutedViewHost()
        {
            this.WhenActivated(disposables =>
            {
                var routerRemoved = this
                    .WhenAnyValue(x => x.Router)
                    .Where(router => router == null)!
                    .Cast<object?>();

                var viewContract = this.WhenAnyValue(x => x.ViewContract);

                this.WhenAnyValue(x => x.Router)
                    .Where(router => router != null)
                    // Changing .SelectMany() here to a chained .Select().Switch() seems to fix the desync.
                    .Select(router => router!.CurrentViewModel)
                    .Switch()
                    .Merge(routerRemoved)
                    .CombineLatest(viewContract)
                    .Subscribe(tuple => NavigateToViewModel(tuple.First, tuple.Second))
                    .DisposeWith(disposables);
            });
        }

        /// <summary>
        /// Gets or sets the <see cref="RoutingState"/> of the view model stack.
        /// </summary>
        public RoutingState? Router
        {
            get => GetValue(RouterProperty);
            set => SetValue(RouterProperty, value);
        }

        /// <summary>
        /// Gets or sets the view contract.
        /// </summary>
        public string? ViewContract
        {
            get => GetValue(ViewContractProperty);
            set => SetValue(ViewContractProperty, value);
        }

        /// <summary>
        /// Gets or sets the content displayed whenever there is no page currently routed.
        /// </summary>
        public object? DefaultContent
        {
            get => GetValue(DefaultContentProperty);
            set => SetValue(DefaultContentProperty, value);
        }

        /// <summary>
        /// Gets or sets the ReactiveUI view locator used by this router.
        /// </summary>
        public IViewLocator? ViewLocator { get; set; }

        protected override Type StyleKeyOverride => typeof(TransitioningContentControl);

        /// <summary>
        /// Invoked when ReactiveUI router navigates to a view model.
        /// </summary>
        /// <param name="viewModel">ViewModel to which the user navigates.</param>
        /// <param name="contract">The contract for view resolution.</param>
        private void NavigateToViewModel(object? viewModel, string? contract)
        {
            if (Router == null)
            {
                this.Log().Warn("Router property is null. Falling back to default content.");
                Content = DefaultContent;
                return;
            }

            if (viewModel == null)
            {
                this.Log().Info("ViewModel is null. Falling back to default content.");
                Content = DefaultContent;
                return;
            }

            var viewLocator = ViewLocator ?? global::ReactiveUI.ViewLocator.Current;
            var viewInstance = viewLocator.ResolveView(viewModel, contract);
            if (viewInstance == null)
            {
                if (contract == null)
                {
                    this.Log().Warn($"Couldn't find view for '{viewModel}'. Is it registered? Falling back to default content.");
                }
                else
                {
                    this.Log().Warn($"Couldn't find view with contract '{contract}' for '{viewModel}'. Is it registered? Falling back to default content.");
                }

                Content = DefaultContent;
                return;
            }

            if (contract == null)
            {
                this.Log().Info($"Ready to show {viewInstance} with autowired {viewModel}.");
            }
            else
            {
                this.Log().Info($"Ready to show {viewInstance} with autowired {viewModel} and contract '{contract}'.");
            }

            viewInstance.ViewModel = viewModel;
            if (viewInstance is IDataContextProvider provider)
                provider.DataContext = viewModel;
            Content = viewInstance;
        }
    }
}
