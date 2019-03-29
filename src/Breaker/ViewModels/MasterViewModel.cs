using System.ComponentModel;
using Autofac;

namespace Breaker.ViewModels
{
    using Caliburn.Micro;
    using Events;
    using PropertyChanged;

    /// <summary>
    /// Handles screens management and wiring
    /// </summary>
    public class MasterViewModel : Conductor<object>, IShell, IHandle<ChangeMainScreenEvent>
    {
        /// <summary>
        /// Stores the events aggregator
        /// </summary>
        private readonly IEventAggregator events;

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterViewModel"/> class
        /// </summary>
        /// <param name="events">The event aggregator</param>
        public MasterViewModel(IEventAggregator events, ILifetimeScope scope)
        {
            DisplayName = "Breaker";
            this.events = events;

            ActivateItem(scope.Resolve<MainViewModel>());
            this.events.Subscribe(this);
        }

        /// <summary>
        /// Handles the change of screen
        /// </summary>
        /// <param name="message">The event message</param>
        public void Handle(ChangeMainScreenEvent message)
        {
            DisplayName = message.Screen.DisplayName;
            ActivateItem(message.Screen);
        }
    }
}
