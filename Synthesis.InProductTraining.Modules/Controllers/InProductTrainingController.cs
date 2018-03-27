using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.Cache;
using Synthesis.DocumentStorage;
using Synthesis.EventBus;
using Synthesis.Logging;
using Synthesis.Nancy.MicroService;
using Synthesis.Nancy.MicroService.Validation;
using Synthesis.InProductTrainingService.Constants;
using Synthesis.InProductTrainingService.Data;
using Synthesis.InProductTrainingService.Enums;
using Synthesis.InProductTrainingService.InternalApi.Enums;
using Synthesis.InProductTrainingService.InternalApi.Models;
using Synthesis.InProductTrainingService.InternalApi.Requests;
using Synthesis.InProductTrainingService.InternalApi.Responses;
using Synthesis.InProductTrainingService.Resolvers;
using Synthesis.InProductTrainingService.Validators;
using Synthesis.TenantService.InternalApi.Api;
//using Synthesis.PolicyEvaluator.Constants;
using Synthesis.Serialization;

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
        private readonly IRepository<InProductTrainingView> _inProductTrainingRepository;
        private readonly IValidatorLocator _validatorLocator;
        private readonly IObjectSerializer _serializer;
        private readonly ICache _cache;
        private readonly InProductTrainingSqlService _dbService;
        private readonly ITenantApi _tenantApi;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InProductTrainingController" /> class.
        /// </summary>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="validatorLocator">The validator locator.</param>
        /// <param name="eventService">The event service.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="serializer"></param>
        /// <param name="cache"></param>
        /// <param name="tenantApi"></param>
        public InProductTrainingController(
            IRepositoryFactory repositoryFactory,
            IValidatorLocator validatorLocator,
            IEventService eventService,
            ILoggerFactory loggerFactory,
            IObjectSerializer serializer,
            ICache cache,
            ITenantApi tenantApi)
        {
            _inProductTrainingRepository = repositoryFactory.CreateRepository<InProductTrainingView>();
            _validatorLocator = validatorLocator;
            _eventService = eventService;
            _logger = loggerFactory.GetLogger(this);
            _serializer = serializer;
            _cache = cache;

            _dbService = new InProductTrainingSqlService();
            _tenantApi = tenantApi;
        }

        public async Task<InProductTrainingViewResponse> CreateInProductTrainingViewAsync(InProductTrainingViewRequest model, Guid tenantId)
        {
            await Task.Yield();

            Guid userId;
            try
            {
                userId = _tenantApi.GetTenantByIdAsync(tenantId).Result.Payload.Id.GetValueOrDefault();
            }
            catch (NotFoundException ex)
            {
                _logger.Error("A UserId could not be found for this create request", ex);
                return new InProductTrainingViewResponse { ReturnCode = CreateInProductTrainingViewReturnCode.UserNotFound };
            }
            catch (Exception ex)
            {
                _logger.Error("An unknown exception was encountered when trying to retrieve UserId could not be found for this create request", ex);
                return new InProductTrainingViewResponse { ReturnCode = CreateInProductTrainingViewReturnCode.CreateFailed };
            }

            //var result = new ServiceResult<InProductTrainingViewResponseDto>();
            //try
            //{
            //    var key = KeyResolver.InProductTrainingViews(userId, inProductTrainingViewRequestDto.ClientApplicationId);
            //    var cache = _cacheSelector[CacheConnection.General];
            //    var dtoForTrace = _serializer.SerializeToString(inProductTrainingViewRequestDto);

            //    var cachedData = await cache.SetGetMembersAsync<InProductTrainingViewResponseDto>(key);
            //    if (cachedData != null)
            //    {
            //        var trainingOfType = cachedData?.Where(t =>
            //                t.InProductTrainingSubjectId == inProductTrainingViewRequestDto.InProductTrainingSubjectId &&
            //                t.Title == inProductTrainingViewRequestDto.Title &&
            //                t.UserId == userId)
            //            .FirstOrDefault();

            //        if (trainingOfType != null)
            //        {
            //            result.Payload = trainingOfType;
            //            result.Message = CreateInProductTrainingViewReturnCode.RecordAlreadyExists.BuildResponseMessage(inProductTrainingViewRequestDto, userId);
            //            result.ResultCode = ResultCode.RecordAlreadyExists;

            //            _loggingService.LogInfo(LogTopic.WEBAPI, $"{nameof(CreateInProductTrainingView)} - Record not created because it already exists in cache. {dtoForTrace}");
            //        }
            //    }

            //    var populateCache = false;
            //    InProductTrainingViewResponseDto queryResult = null;
            //    if (result.ResultCode != ResultCode.RecordAlreadyExists)
            //    {
            //        // Database
            //        CreateInProductTrainingViewReturnCode returnCode = CreateInProductTrainingViewReturnCode.CreateFailed;
            //        queryResult = _databaseService.CreateInProductTrainingView(
            //            inProductTrainingViewRequestDto.InProductTrainingSubjectId, userId,
            //            inProductTrainingViewRequestDto.Title, createdByUserName, ref returnCode);

            //        result.Payload = queryResult;
            //        result.Message = returnCode.BuildResponseMessage(inProductTrainingViewRequestDto, userId);
            //        result.ResultCode = returnCode.ToResultCode();

            //        if (returnCode == CreateInProductTrainingViewReturnCode.CreateSucceeded)
            //        {
            //            populateCache = true;
            //            _loggingService.Log(LogTopic.WEBAPI,
            //                $"{nameof(CreateInProductTrainingView)} - Created InProductTrainingView record. {dtoForTrace}");
            //        }
            //        else
            //        {
            //            if (returnCode == CreateInProductTrainingViewReturnCode.RecordAlreadyExists)
            //            {
            //                populateCache = true;
            //                _loggingService.LogInfo(LogTopic.WEBAPI, $"{nameof(CreateInProductTrainingView)} - Record not created because it already exists in dB. {dtoForTrace}");
            //            }
            //            else if (returnCode == CreateInProductTrainingViewReturnCode.CreateFailed)
            //            {
            //                _loggingService.LogError(LogTopic.WEBAPI,
            //                    $"{nameof(CreateInProductTrainingView)} - {result.Message} {dtoForTrace}");
            //            }
            //            else
            //            {
            //                _loggingService.LogWarning(LogTopic.WEBAPI,
            //                    $"{nameof(CreateInProductTrainingView)} - {result.Message} {dtoForTrace}");
            //            }
            //        }
            //    }

            //    if (populateCache && queryResult != null)
            //    {
            //        var queryResultAsList = new List<InProductTrainingViewResponseDto>();
            //        queryResultAsList.Add(queryResult);
            //        if (await cache.SetAddAsync(key, queryResultAsList) > 0 || await cache.KeyExistsAsync(key))
            //        {
            //            _loggingService.Log(LogTopic.WEBAPI, $"{nameof(CreateInProductTrainingView)} - Succesfully cached an item in the set for key '{key}' {dtoForTrace}");
            //            if (!await cache.KeySetExpirationAsync(key, _expirationTime, CacheCommandOptions.None))
            //            {
            //                _loggingService.LogError(LogTopic.WEBAPI, $"{nameof(CreateInProductTrainingView)} - Could not set cache expiration for the key '{key}' or the key does not exist. {dtoForTrace}");
            //            }
            //        }
            //        else
            //        {
            //            _loggingService.LogError(LogTopic.WEBAPI, $"{nameof(CreateInProductTrainingView)} - Could not cache an item in the set for '{key}'. {dtoForTrace}");
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    result.Payload = null;
            //    result.Message = ex.ToString();
            //    result.ResultCode = ResultCode.Failed;

            //    _loggingService.LogError(LogTopic.WEBAPI, ex, $"{nameof(CreateInProductTrainingView)}");
            //}

            return null;
        }

        public async Task<List<InProductTrainingViewResponse>> GetViewedInProductTrainingAsync(int clientApplicationId)
        {
            await Task.Yield();

            //var result = new ServiceResult<List<InProductTrainingViewResponseDto>>();
            //try
            //{
            //    string key = KeyResolver.InProductTrainingViews(userId, clientApplicationId);
            //    var cache = _cacheSelector[CacheConnection.General];

            //    // do not use 
            //    if (await cache.KeyExistsAsync(key))
            //    {
            //        var cachedData = await cache.SetGetMembersAsync<InProductTrainingViewResponseDto>(key);

            //        if (cachedData != null)
            //        {
            //            result.Payload = cachedData.ToList();
            //            result.Message = string.Empty;
            //            result.ResultCode = ResultCode.Success;

            //            _loggingService.Log(LogTopic.WEBAPI, $"{nameof(GetInProductTrainingViewsAsync)} - Item retrieved from cache for key {key}.");
            //        }
            //    }

            //    if (result.ResultCode != ResultCode.Success)
            //    {
            //        var dbData = await _databaseService.GetInProductTrainingViewsAsync(userId, clientApplicationId);

            //        if (dbData != null)
            //        {
            //            result.Payload = dbData;
            //            result.Message = string.Empty;
            //            result.ResultCode = ResultCode.Success;

            //            if (dbData.Any())
            //            {
            //                _loggingService.Log(LogTopic.WEBAPI,
            //                    $"{nameof(GetInProductTrainingViewsAsync)} - Records retrieved from dB for key {key}.");

            //                // Cache
            //                if (await cache.SetAddAsync(key, dbData) > 0 || await cache.KeyExistsAsync(key))
            //                {
            //                    _loggingService.Log(LogTopic.WEBAPI,
            //                        $"{nameof(GetInProductTrainingViewsAsync)} - Succesfully cached an item in the set for key '{key}'.");
            //                    if (!await cache.KeySetExpirationAsync(key, _expirationTime, CacheCommandOptions.None))
            //                    {
            //                        _loggingService.LogError(LogTopic.WEBAPI,
            //                            $"{nameof(GetInProductTrainingViewsAsync)} - Could not set cache expiration for the key '{key}' or the key does not exist.");
            //                    }
            //                }
            //                else
            //                {
            //                    _loggingService.LogError(LogTopic.WEBAPI,
            //                        $"{nameof(GetInProductTrainingViewsAsync)} - Could not cache an item in the set for '{key}'.");
            //                }
            //            }
            //        }
            //        else
            //        {
            //            var errorMessage =
            //                $"{nameof(GetInProductTrainingViewsAsync)} - Unable to retrieve in-product training views from database. Args [{nameof(userId)}: {userId}, {nameof(clientApplicationId)}: {clientApplicationId}]";

            //            result.Payload = null;
            //            result.Message = errorMessage;
            //            result.ResultCode = ResultCode.Failed;

            //            _loggingService.LogError(LogTopic.WEBAPI, $"{nameof(CreateInProductTrainingView)} - {errorMessage}");
            //        }

            //    }
            //}
            //catch (Exception ex)
            //{
            //    var errorMessage =
            //        $"{nameof(GetInProductTrainingViewsAsync)} - Unable to retrieve in-product training views from database. Args [{nameof(userId)}: {userId}, {nameof(clientApplicationId)}: {clientApplicationId}]";

            //    result.Payload = null;
            //    result.Message = errorMessage;
            //    result.ResultCode = ResultCode.Failed;

            //    _loggingService.LogError(LogTopic.WEBAPI, ex, $"{errorMessage}");
            //}

            return null;
        }

        private string BuildCreateInProductTrainingViewResponseMessage(CreateInProductTrainingViewReturnCode returnCode, InProductTrainingViewRequest inProductTrainingViewRequestDto, Guid userId)
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
                    responseMessage = $"Validation failed. {baseError} {nameof(InProductTrainingViewRequest.InProductTrainingSubjectId)} {inProductTrainingViewRequestDto.InProductTrainingSubjectId} could not be found.";
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