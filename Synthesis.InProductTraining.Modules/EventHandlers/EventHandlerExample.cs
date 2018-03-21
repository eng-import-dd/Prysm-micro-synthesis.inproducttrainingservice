using System;
using Synthesis.EventBus;
using Synthesis.Logging;

namespace Synthesis.InProductTrainingService.EventHandlers
{
    public class EventHandlerExample : IEventHandler<object>
    {
        private readonly ILogger _logger;

        public EventHandlerExample(ILoggerFactory factory)
        {
            _logger = factory.GetLogger(this);
        }

        /// <inheritdoc />
        public void HandleEvent(object args)
        {
            try
            {
                //TODO do something here, probably inject a controller into the constructor and mess with it
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }
    }
}
