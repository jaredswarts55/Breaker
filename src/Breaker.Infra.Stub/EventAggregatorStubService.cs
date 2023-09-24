using Breaker.Core.Events;

namespace Breaker.Infra.Stub
{
    public class EventAggregatorStubService : IEventAggregator
    {
        public void PublishOnUIThread(object obj)
        {
        }

        public void Subscribe(object mainWindowViewModel)
        {
        }
    }
}