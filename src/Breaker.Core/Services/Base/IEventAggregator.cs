using System.Threading.Tasks;

namespace Breaker.Core.Events
{
    public interface IEventAggregator
    {
        public void PublishOnUIThread(object obj);
    }
}