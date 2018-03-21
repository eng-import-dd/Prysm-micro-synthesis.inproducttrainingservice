using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.DocumentStorage;
using Synthesis.EventBus;
using Synthesis.Logging;
using Synthesis.Nancy.MicroService;
using Synthesis.Nancy.MicroService.Validation;
using Synthesis.InProductTrainingService.Constants;
using Synthesis.InProductTrainingService.Models;
using Synthesis.InProductTrainingService.Validators;

namespace Synthesis.InProductTrainingService.Controllers
{
    /// <summary>
    ///     Represents a controller for InProductTraining resources.
    /// </summary>
    /// <seealso cref="IInProductTrainingController" />
    public class InProductTrainingController : IInProductTrainingController
    {
        private readonly IEventService _eventService;
        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger _logger;
        private readonly IRepository<InProductTraining> _inProductTrainingRepository;
        private readonly IValidatorLocator _validatorLocator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InProductTrainingController" /> class.
        /// </summary>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="validatorLocator">The validator locator.</param>
        /// <param name="eventService">The event service.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public InProductTrainingController(
            IRepositoryFactory repositoryFactory,
            IValidatorLocator validatorLocator,
            IEventService eventService,
            ILoggerFactory loggerFactory)
        {
            _inProductTrainingRepository = repositoryFactory.CreateRepository<InProductTraining>();
            _validatorLocator = validatorLocator;
            _eventService = eventService;
            _logger = loggerFactory.GetLogger(this);
        }

        public async Task<InProductTraining> CreateInProductTrainingAsync(InProductTraining model)
        {
            var validationResult = _validatorLocator.Validate<InProductTrainingValidator>(model);
            if (!validationResult.IsValid)
            {
                throw new ValidationFailedException(validationResult.Errors);
            }

            var result = await _inProductTrainingRepository.CreateItemAsync(model);

            _eventService.Publish(EventNames.InProductTrainingCreated, result);

            return result;
        }

        public async Task DeleteInProductTrainingAsync(Guid id)
        {
            var validationResult = _validatorLocator.Validate<InProductTrainingIdValidator>(id);
            if (!validationResult.IsValid)
            {
                throw new ValidationFailedException(validationResult.Errors);
            }

            try
            {
                await _inProductTrainingRepository.DeleteItemAsync(id);

                _eventService.Publish(new ServiceBusEvent<Guid>
                {
                    Name = EventNames.InProductTrainingDeleted,
                    Payload = id
                });
            }
            catch (DocumentNotFoundException)
            {
                // We don't really care if it's not found.
                // The resource not being there is what we wanted.
            }
        }

        public async Task<InProductTraining> GetInProductTrainingAsync(Guid id)
        {
            var validationResult = _validatorLocator.Validate<InProductTrainingIdValidator>(id);
            if (!validationResult.IsValid)
            {
                throw new ValidationFailedException(validationResult.Errors);
            }

            var result = await _inProductTrainingRepository.GetItemAsync(id);

            if (result == null)
            {
                throw new NotFoundException($"A InProductTraining resource could not be found for id {id}");
            }

            return result;
        }

        public async Task<InProductTraining> UpdateInProductTrainingAsync(Guid inProductTrainingId, InProductTraining inProductTrainingModel)
        {
            var validationResult = _validatorLocator.ValidateMany(new Dictionary<Type, object>
            {
                { typeof(InProductTrainingIdValidator), inProductTrainingId },
                { typeof(InProductTrainingValidator), inProductTrainingModel }
            });
            if (!validationResult.IsValid)
            {
                throw new ValidationFailedException(validationResult.Errors);
            }

            try
            {
                var result = await _inProductTrainingRepository.UpdateItemAsync(inProductTrainingId, inProductTrainingModel);

                _eventService.Publish(EventNames.InProductTrainingUpdated, result);

                return result;
            }
            catch (DocumentNotFoundException)
            {
                throw new NotFoundException($"A InProductTraining resource could not be found for id {inProductTrainingId}");
            }
        }
    }
}
