using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Synthesis.Cache;
using Synthesis.Logging;
using Synthesis.Nancy.MicroService.Validation;
using Synthesis.InProductTrainingService.Data;
using Synthesis.InProductTrainingService.InternalApi.Enums;
using Synthesis.InProductTrainingService.InternalApi.Models;
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
        private readonly TimeSpan _expirationTime = TimeSpan.FromHours(8);
        private readonly IUserApi _userApi;

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

            var returnPayload = new InProductTrainingViewResponse();
            var returnMessage = "";
            var returnResultCode = ResultCode.Failed;

            var createdByUserName = _userApi.GetUserAsync(userId).Result.Payload.Username;
            if (createdByUserName.IsNullOrEmpty())
            {
                createdByUserName = "Api";
            }

            try
            {
                var key = KeyResolver.InProductTrainingViews(userId, inProductTrainingViewRequest.ClientApplicationId);
                var dtoForTrace = _serializer.SerializeToString(inProductTrainingViewRequest);

                var cachedData = await _cache.SetMembersAsync<InProductTrainingViewResponse>(key);
                // ReSharper disable once UseNullPropagation
                if (cachedData != null)
                {
                    var trainingOfType = cachedData?.Where(t =>
                            t.InProductTrainingSubjectId == inProductTrainingViewRequest.InProductTrainingSubjectId &&
                            t.Title == inProductTrainingViewRequest.Title &&
                            t.UserId == userId)
                        .FirstOrDefault();

                    if (trainingOfType != null)
                    {
                        returnPayload = trainingOfType;
                        returnMessage = CreateInProductTrainingViewReturnCode.RecordAlreadyExists.BuildResponseMessage(inProductTrainingViewRequest, userId);
                        returnResultCode = ResultCode.RecordAlreadyExists;

                        _logger.Info($"Record not created because it already exists in cache. {dtoForTrace}");
                    }
                }

                var populateCache = false;
                InProductTrainingViewResponse queryResult = null;
                if (returnResultCode != ResultCode.RecordAlreadyExists)
                {
                    var returnCode = CreateInProductTrainingViewReturnCode.CreateFailed;
                    queryResult = _dbService.CreateInProductTrainingView(
                        inProductTrainingViewRequest.InProductTrainingSubjectId, userId,
                        inProductTrainingViewRequest.Title, inProductTrainingViewRequest.UserTypeId, createdByUserName, ref returnCode);

                    returnPayload = queryResult;
                    returnMessage = returnCode.BuildResponseMessage(inProductTrainingViewRequest, userId);
                    returnResultCode = returnCode.ToResultCode();

                    if (returnCode == CreateInProductTrainingViewReturnCode.CreateSucceeded)
                    {
                        populateCache = true;
                        _logger.Info($"Created InProductTrainingView record. {dtoForTrace}");
                    }
                    else
                    {
                        if (returnCode == CreateInProductTrainingViewReturnCode.RecordAlreadyExists)
                        {
                            populateCache = true;
                            _logger.Info($"Record not created because it already exists in dB. {dtoForTrace}");
                        }
                        else if (returnCode == CreateInProductTrainingViewReturnCode.CreateFailed)
                        {
                            _logger.Error($"{returnMessage} {dtoForTrace}");
                        }
                        else
                        {
                            _logger.Warning($"{returnMessage} {dtoForTrace}");
                        }
                    }
                }

                if (populateCache && queryResult != null)
                {
                    var queryResultAsList = new List<InProductTrainingViewResponse> { queryResult };
                    if (await _cache.SetAddAsync(key, queryResultAsList) > 0 || await _cache.KeyExistsAsync(key))
                    {
                        _logger.Info($"Succesfully cached an item in the set for key '{key}' {dtoForTrace}");
                        if (!await _cache.KeyExpireAsync(key, _expirationTime, CacheCommandOptions.None))
                        {
                            _logger.Error($"Could not set cache expiration for the key '{key}' or the key does not exist. {dtoForTrace}");
                        }
                    }
                    else
                    {
                        _logger.Error($"Could not cache an item in the set for '{key}'. {dtoForTrace}");
                    }
                }
            }
            catch (Exception ex)
            {
                returnPayload.ResultCode = ResultCode.Failed;
                returnPayload.ReturnMessage = ex.ToString();

                _logger.Error("Create InProductTrainingView failed due to an unknown exception", ex);
            }

            returnPayload.ReturnMessage = returnMessage;
            returnPayload.ResultCode = returnResultCode;
            return returnPayload;
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



        public async Task<WizardViewResponse> CreateWizardViewAsync(ViewedWizard model)
        {
            var validationResult = _validatorLocator.Validate<ViewedWizardValidator>(model);

            if (!validationResult.IsValid)
            {
                _logger.Error("Validation failed while attempting to create a ViewedWizard resource.");
                throw new ValidationFailedException(validationResult.Errors);
            }

            try
            {
                var existingWizards = await RetrieveViewedWizardsAsync(model.UserId);
                var wizardsOfType = existingWizards.Where(w => w.WizardType == model.WizardType).ToList();

                if (wizardsOfType.Count > 0)
                {
                    return new WizardViewResponse
                    {
                        WizardViews = wizardsOfType,
                        ResultMessage = $"Wizard(s) already viewed by user '{model.UserId}'.",
                        ResultCode = ResultCode.RecordAlreadyExists
                    };
                }

                var viewedWizard = await _dbService.CreateViewedWizardAsync(model);
                var key = KeyResolver.ViewedWizards(model.UserId);
                await _cache.KeyDeleteAsync(key, CacheCommandOptions.FireAndForget);

                return new WizardViewResponse
                {
                    WizardViews = new List<ViewedWizard> { viewedWizard },
                    ResultMessage = $"Wizard marked as viewed successfully for user '{model.UserId}'.",
                    ResultCode = ResultCode.Success
                };
            }
            catch (Exception ex)
            {
                _logger.Error("A ViewedWizard resource has not been created due to an unknown error.", ex);
                return new WizardViewResponse()
                {
                    ResultMessage = ex.ToString(),
                    ResultCode = ResultCode.Failed
                };
            }
        }

        public async Task<WizardViewResponse> GetWizardViewsByUserIdAsync(Guid userId)
        {
            var validationResult = _validatorLocator.Validate<UserIdValidator>(userId);

            if (!validationResult.IsValid)
            {
                _logger.Error("Validation failed while attempting to retrieve a ViewedWizard resource.");
                throw new ValidationFailedException(validationResult.Errors);
            }

            try
            {
                return new WizardViewResponse
                {
                    WizardViews = await RetrieveViewedWizardsAsync(userId),
                    ResultMessage = $"Successfully retrieved ViewedWizards for user {userId}",
                    ResultCode = ResultCode.Success
                };
            }
            catch (Exception ex)
            {
                _logger.Error("A ViewedWizard resource could not be retrieved due to an unknown error.", ex);
                return new WizardViewResponse()
                {
                    WizardViews = null,
                    ResultMessage = ex.ToString(),
                    ResultCode = ResultCode.Failed
                };
            }
        }

        private async Task<List<ViewedWizard>> RetrieveViewedWizardsAsync(Guid userId)
        {
            var key = KeyResolver.ViewedWizards(userId);

            if (_cache.KeyExists(key))
            {
                return await _cache.ItemGetAsync<List<ViewedWizard>>(key);
            }

            var wizardViews = await _dbService.GetViewedWizardsAsync(userId);
            await _cache.ItemSetAsync(key, wizardViews, _expirationTime);

            return wizardViews;
        }
    }
}