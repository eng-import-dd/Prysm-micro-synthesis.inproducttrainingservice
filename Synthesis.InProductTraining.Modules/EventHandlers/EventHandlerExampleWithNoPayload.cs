using System;
using Synthesis.EventBus;
using Synthesis.Logging;

namespace Synthesis.InProductTrainingService.EventHandlers
{
    public class EventHandlerExampleWithNoPayload : IEventHandler
    {
        private readonly ILogger _logger;

        public EventHandlerExampleWithNoPayload(ILoggerFactory factory)
        {
            _logger = factory.GetLogger(this);
        }

        /// <inheritdoc />
        public void HandleEvent()
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
