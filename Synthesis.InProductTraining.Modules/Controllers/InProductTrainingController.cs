using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Threading.Tasks;
using Synthesis.Cache;
using Synthesis.Logging;
using Synthesis.Nancy.MicroService;
using Synthesis.Nancy.MicroService.Validation;
using Synthesis.InProductTrainingService.Data;
using Synthesis.InProductTrainingService.InternalApi.Enums;
using Synthesis.InProductTrainingService.InternalApi.Requests;
using Synthesis.InProductTrainingService.InternalApi.Responses;
using Synthesis.InProductTrainingService.Resolvers;
using Synthesis.InProductTrainingService.Validators;
using Synthesis.PrincipalService.InternalApi.Api;
using Synthesis.Serialization;

namespace Synthesis.InProductTrainingService.Controllers
{
    /// <summary>
    ///     Represents a controller for InProductTraining resources.
    /// </summary>
    /// <seealso cref="IInProductTrainingController" />
    public class InProductTrainingController : IInProductTrainingController
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger _logger;
        private readonly IValidatorLocator _validatorLocator;
        private readonly IObjectSerializer _serializer;
        private readonly ICache _cache;
        private readonly InProductTrainingSqlService _dbService;
        private readonly IUserApi _userApi;
        private readonly TimeSpan _expirationTime = TimeSpan.FromHours(8);

        /// <summary>
        ///     Initializes a new instance of the <see cref="InProductTrainingController" /> class.
        /// </summary>
        /// <param name="validatorLocator">The validator locator.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="serializer"></param>
        /// <param name="cache"></param>
        /// <param name="userApi"></param>
        public InProductTrainingController(
            IValidatorLocator validatorLocator,
            ILoggerFactory loggerFactory,
            IObjectSerializer serializer,
            ICache cache,
            IUserApi userApi)
        {
            _validatorLocator = validatorLocator;
            _logger = loggerFactory.GetLogger(this);
            _serializer = serializer;
            _cache = cache;

            _dbService = new InProductTrainingSqlService();
            _userApi = userApi;
        }

        public async Task<InProductTrainingViewResponse> CreateInProductTrainingViewAsync(InProductTrainingViewRequest inProductTrainingViewRequest, Guid userId)
        {
            var validationResult = _validatorLocator.Validate<InProductTrainingViewRequestValidator>(inProductTrainingViewRequest);

            if (!validationResult.IsValid)
            {
                _logger.Error("Validation failed while attempting to create an InProductTrainingView resource.");
                throw new ValidationFailedException(validationResult.Errors);
            }

            var createdByUserName = _userApi.GetUserAsync(userId).Result.Payload.Username;
            var key = KeyResolver.InProductTrainingViews(userId, inProductTrainingViewRequest.ClientApplicationId);
            var dtoForTrace = _serializer.SerializeToString(inProductTrainingViewRequest);
            string returnMessage;

            var cachedData = await _cache.SetMembersAsync<InProductTrainingViewResponse>(key);
            var trainingOfType = cachedData?.Where(t =>
                    t.InProductTrainingSubjectId == inProductTrainingViewRequest.InProductTrainingSubjectId &&
                    t.Title == inProductTrainingViewRequest.Title &&
                    t.UserId == userId)
                .FirstOrDefault();

            if (trainingOfType != null)
            {
                PopulateCache(trainingOfType, key, dtoForTrace);
                returnMessage = $"{nameof(CreateInProductTrainingViewAsync)} -  Record not created because it already exists in cache. {dtoForTrace}";

                throw new Exception(returnMessage);
            }

            // Database
            var returnCode = CreateInProductTrainingViewReturnCode.CreateFailed;
            var queryResult = _dbService.CreateInProductTrainingView(
                inProductTrainingViewRequest.InProductTrainingSubjectId, userId,
                inProductTrainingViewRequest.Title, inProductTrainingViewRequest.UserTypeId, createdByUserName, ref returnCode);

            switch (returnCode)
            {
                case CreateInProductTrainingViewReturnCode.CreateSucceeded:

                    if (queryResult != null)
                    {
                        PopulateCache(queryResult, key, dtoForTrace);
                    }

                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, userId);
                    _logger.Info(returnMessage);

                    return queryResult;

                case CreateInProductTrainingViewReturnCode.RecordAlreadyExists:

                    if (queryResult != null)
                    {
                        PopulateCache(queryResult, key, dtoForTrace);
                    }

                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, userId);
                    _logger.Info(returnMessage);

                    throw new Exception(returnMessage);

                case CreateInProductTrainingViewReturnCode.CreateFailed:

                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, userId);
                    _logger.Error(returnMessage);

                    throw new RequestFailedException(returnMessage);

                case CreateInProductTrainingViewReturnCode.InProductTrainingSubjectNotFound:

                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, userId);
                    _logger.Error(returnMessage);

                    throw new NotFoundException(returnMessage);

                case CreateInProductTrainingViewReturnCode.UserNotFound:

                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, userId);
                    _logger.Error(returnMessage);

                    throw new NotFoundException(returnMessage);

                default:

                    returnMessage = BuildCreateInProductTrainingViewResponseMessage(returnCode, inProductTrainingViewRequest, userId);

                    throw new Exception(returnMessage);
            }
        }

        public async Task<IEnumerable<InProductTrainingViewResponse>> GetViewedInProductTrainingAsync(int clientApplicationId, Guid userId)
        {
            var validationResult = _validatorLocator.Validate<ClientApplicationIdValidator>(clientApplicationId);

            if (!validationResult.IsValid)
            {
                _logger.Error("Validation failed while attempting to create an InProductTrainingView resource.");
                throw new ValidationFailedException(validationResult.Errors);
            }

            try
            {
                var key = KeyResolver.InProductTrainingViews(userId, clientApplicationId);

                if (await _cache.KeyExistsAsync(key))
                {
                    var cachedData = await _cache.SetMembersAsync<InProductTrainingViewResponse>(key);

                    if (cachedData != null)
                    {
                        _logger.Info($"{nameof(GetViewedInProductTrainingAsync)} - Item retrieved from cache for key {key}.");
                        return cachedData.ToList();
                    }
                }

                var dbData = await _dbService.GetInProductTrainingViewsAsync(clientApplicationId, userId);

                if (dbData != null)
                {
                    if (dbData.Any())
                    {
                        _logger.Info($"{nameof(GetViewedInProductTrainingAsync)} - Records retrieved from dB for key {key}.");

                        if (await _cache.SetAddAsync(key, dbData) > 0 || await _cache.KeyExistsAsync(key))
                        {
                            _logger.Info($"{nameof(GetViewedInProductTrainingAsync)} - Succesfully cached an item in the set for key '{key}'.");

                            if (!await _cache.KeyExpireAsync(key, _expirationTime, CacheCommandOptions.None))
                            {
                                _logger.Info($"{nameof(GetViewedInProductTrainingAsync)} - Could not set cache expiration for the key '{key}' or the key does not exist.");
                            }
                        }
                        else
                        {
                            _logger.Error($"{nameof(GetViewedInProductTrainingAsync)} - Could not cache an item in the set for '{key}'.");
                        }
                    }
                }
                else
                {
                    var errorMessage =
                        $"{nameof(GetViewedInProductTrainingAsync)} - Unable to retrieve in-product training views from database. Args " +
                        $"[{nameof(userId)}: {userId}, {nameof(clientApplicationId)}: {clientApplicationId}]";

                    throw new Exception(errorMessage);
                }

                return dbData;
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"{nameof(GetViewedInProductTrainingAsync)} - Unable to retrieve in-product training views from database. Args " +
                    $"[{nameof(userId)}: {userId}, {nameof(clientApplicationId)}: {clientApplicationId}]";

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