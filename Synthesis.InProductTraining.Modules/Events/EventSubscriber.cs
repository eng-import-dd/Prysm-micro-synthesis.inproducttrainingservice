using Synthesis.EventBus;
using Synthesis.InProductTrainingService.EventHandlers;

namespace Synthesis.InProductTrainingService.Events
{
    public class EventSubscriber
    {
        public EventSubscriber(IEventHandlerLocator eventHandlerLocator)
        {
            //TODO
            eventHandlerLocator
                .SubscribeEventHandler<EventHandlerExample, object>("the namespace the event is coming from", "the event name")
                .SubscribeEventHandler<EventHandlerExampleWithNoPayload>("the namespace the event is coming from", "the event name");
        }
    }
}
