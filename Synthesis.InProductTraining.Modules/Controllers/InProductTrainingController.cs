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

        public async Task<InProductTraining> CreateInProductTrainingViewAsync(InProductTraining model)
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

        public async Task<InProductTraining> GetViewedInProductTrainingAsync(Guid id)
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
    }
}