using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Threading.Tasks;
using Synthesis.Cache;
//using Synthesis.DocumentStorage;
//using Synthesis.EventBus;
using Synthesis.Logging;
using Synthesis.Nancy.MicroService;
//using Synthesis.Nancy.MicroService.Validation;
using Synthesis.InProductTrainingService.Data;
using Synthesis.InProductTrainingService.InternalApi.Enums;
using Synthesis.InProductTrainingService.InternalApi.Requests;
using Synthesis.InProductTrainingService.InternalApi.Responses;
using Synthesis.InProductTrainingService.Resolvers;
using Synthesis.TenantService.InternalApi.Api;
using Synthesis.Serialization;

namespace Synthesis.InProductTrainingService.Controllers
{
    /// <summary>
    ///     Represents a controller for InProductTraining resources.
    /// </summary>
    /// <seealso cref="IInProductTrainingController" />
    public class InProductTrainingController : IInProductTrainingController
    {
        //private readonly IEventService _eventService;
        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger _logger;
        //private readonly IRepository<InProductTrainingView> _inProductTrainingRepository;
        //private readonly IValidatorLocator _validatorLocator;
        private readonly IObjectSerializer _serializer;
        private readonly ICache _cache;
        private readonly InProductTrainingSqlService _dbService;
        private readonly ITenantApi _tenantApi;
        private readonly TimeSpan _expirationTime = TimeSpan.FromHours(8);

        /// <summary>
        ///     Initializes a new instance of the <see cref="InProductTrainingController" /> class.
        /// </summary>
        ///// <param name="repositoryFactory">The repository factory.</param>
        ///// <param name="validatorLocator">The validator locator.</param>
        ///// <param name="eventService">The event service.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="serializer"></param>
        /// <param name="cache"></param>
        /// <param name="tenantApi"></param>
        public InProductTrainingController(
            //IRepositoryFactory repositoryFactory,
            //IValidatorLocator validatorLocator,
            //IEventService eventService,
            ILoggerFactory loggerFactory,
            IObjectSerializer serializer,
            ICache cache,
            ITenantApi tenantApi)
        {
            //_inProductTrainingRepository = repositoryFactory.CreateRepository<InProductTrainingView>();
            //_validatorLocator = validatorLocator;
            //_eventService = eventService;
            _logger = loggerFactory.GetLogger(this);
            _serializer = serializer;
            _cache = cache;

            _dbService = new InProductTrainingSqlService();
            _tenantApi = tenantApi;
        }

        public async Task<InProductTrainingViewResponse> CreateInProductTrainingViewAsync(InProductTrainingViewRequest inProductTrainingViewRequest, Guid tenantId)
        {
            var createdByUserName = _tenantApi.GetTenantByIdAsync(tenantId).Result.Payload.Name;
            var key = KeyResolver.InProductTrainingViews(tenantId, inProductTrainingViewRequest.ClientApplicationId);
            var dtoForTrace = _serializer.SerializeToString(inProductTrainingViewRequest);

            var cachedData = await _cache.SetMembersAsync<InProductTrainingViewResponse>(key);
            var trainingOfType = cachedData?.Where(t =>
                    t.InProductTrainingSubjectId == inProductTrainingViewRequest.InProductTrainingSubjectId &&
                    t.Title == inProductTrainingViewRequest.Title &&
                    t.UserId == tenantId)
                .FirstOrDefault();

            string returnMessage;

            // If the key already exists in cache, throw the exception
            if (trainingOfType != null)
            {
                PopulateCache(trainingOfType, key, dtoForTrace);
                returnMessage = $"{nameof(CreateInProductTrainingViewAsync)} -  Record not created because it already exists in cache. {dtoForTrace}";
                _logger.Info(returnMessage);

                throw new Exception(returnMessage);
            }

            // Database
            var returnCode = CreateInProductTrainingViewReturnCode.CreateFailed;
            var queryResult = _dbService.CreateInProductTrainingView(
                inProductTrainingViewRequest.InProductTrainingSubjectId, tenantId,
                inProductTrainingViewRequest.Title, inProductTrainingViewRequest.UserTypeId, createdByUserName, ref returnCode);

            switch (returnCode)
            {
                case CreateInProductTrainingViewReturnCode.CreateSucceeded:

                    PopulateCache(queryResult, key, dtoForTrace);
                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, tenantId);
                    _logger.Info(returnMessage);

                    return queryResult;

                case CreateInProductTrainingViewReturnCode.RecordAlreadyExists:

                    PopulateCache(queryResult, key, dtoForTrace);
                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, tenantId);
                    _logger.Info(returnMessage);

                    throw new Exception(returnMessage);

                case CreateInProductTrainingViewReturnCode.CreateFailed:

                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, tenantId);
                    _logger.Error(returnMessage);

                    throw new RequestFailedException(returnMessage);

                case CreateInProductTrainingViewReturnCode.InProductTrainingSubjectNotFound:

                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, tenantId);
                    _logger.Warning(returnMessage);

                    throw new NotFoundException(returnMessage);

                case CreateInProductTrainingViewReturnCode.UserNotFound:

                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, tenantId);
                    _logger.Warning(returnMessage);

                    throw new NotFoundException(returnMessage);

                default:

                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, tenantId);
                    _logger.Warning(returnMessage);

                    throw new Exception();
                }
        }

        public async Task<List<InProductTrainingViewResponse>> GetViewedInProductTrainingAsync(int clientApplicationId, Guid tenantId)
        {
            try
            {
                var key = KeyResolver.InProductTrainingViews(tenantId, clientApplicationId);
                var dbData = await _dbService.GetInProductTrainingViewsAsync(clientApplicationId, tenantId);

                var errorMessage =
                    $"{nameof(GetViewedInProductTrainingAsync)} - Unable to retrieve in-product training views from database. Args " +
                    $"[{nameof(tenantId)}: {tenantId}, {nameof(clientApplicationId)}: {clientApplicationId}]";

                if (dbData != null)
                {
                    if (dbData.Any())
                    {
                        _logger.Info($"{nameof(GetViewedInProductTrainingAsync)} - Records retrieved from dB for key {key}.");
                        return dbData;
                    }

                    _logger.Error($"{nameof(GetViewedInProductTrainingAsync)} - {errorMessage}");
                    throw new NotFoundException(errorMessage);
                }

                _logger.Error($"{nameof(GetViewedInProductTrainingAsync)} - {errorMessage}");
                throw new NotFoundException(errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"{nameof(GetViewedInProductTrainingAsync)} - Unable to retrieve in-product training views from database. Args " +
                    $"[{nameof(tenantId)}: {tenantId}, {nameof(clientApplicationId)}: {clientApplicationId}]";

                _logger.Error($"{errorMessage}", ex);
                throw new Exception(errorMessage, ex);
            }
        }

        private async void PopulateCache(InProductTrainingViewResponse queryResult, string key, string dtoForTrace)
        {
            var queryResultAsList = new List<InProductTrainingViewResponse> { queryResult };
            if (await _cache.SetAddAsync(key, queryResultAsList) > 0 || await _cache.KeyExistsAsync(key))
            {
                _logger.Info($"{nameof(CreateInProductTrainingViewAsync)} - Succesfully cached an item in the set for key '{key}' {dtoForTrace}");
                if (!await _cache.KeyExpireAsync(key, _expirationTime, CacheCommandOptions.None))
                {
                    _logger.Error($"{nameof(CreateInProductTrainingViewAsync)} - Could not set cache expiration for the key '{key}' or the key does not exist. {dtoForTrace}");
                }
            }
            else
            {
                _logger.Error($"{nameof(CreateInProductTrainingViewAsync)} - Could not cache an item in the set for '{key}'. {dtoForTrace}");
            }
        }

        private string BuildCreateInProductTrainingViewResponseMessage(CreateInProductTrainingViewReturnCode returnCode, InProductTrainingViewRequest inProductTrainingViewRequest, Guid userId)
        {
            string responseMessage;
            const string baseError = "In-product training view record could not be created.";
            switch (returnCode)
            {
                case CreateInProductTrainingViewReturnCode.CreateSucceeded:
                    responseMessage = "Success";
                    break;
                case CreateInProductTrainingViewReturnCode.UserNotFound:
                    responseMessage = $"Validation failed. {baseError} {nameof(userId)} '{userId}' could not be found.";
                    break;
                case CreateInProductTrainingViewReturnCode.InProductTrainingSubjectNotFound:
                    responseMessage = $"Validation failed. {baseError} {nameof(InProductTrainingViewRequest.InProductTrainingSubjectId)} {inProductTrainingViewRequest.InProductTrainingSubjectId} could not be found.";
                    break;
                case CreateInProductTrainingViewReturnCode.CreateFailed:
                    responseMessage = $"An error occurred. {baseError}";
                    break;
                case CreateInProductTrainingViewReturnCode.RecordAlreadyExists:
                    responseMessage = $"Record already exists. {baseError}";
                    break;
                default:
                    responseMessage = $"An error occurred. {baseError}";
                    break;
            }

            return responseMessage;
        }
    }
}