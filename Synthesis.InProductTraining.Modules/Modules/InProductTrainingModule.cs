using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Synthesis.Logging;
using Synthesis.Nancy.MicroService;
using Synthesis.Nancy.MicroService.Metadata;
using Synthesis.Nancy.MicroService.Modules;
using Synthesis.Nancy.MicroService.Validation;
using Synthesis.PolicyEvaluator;
using Synthesis.InProductTrainingService.Controllers;
using Synthesis.InProductTrainingService.InternalApi.Enums;
using Synthesis.InProductTrainingService.InternalApi.Requests;
using Synthesis.InProductTrainingService.InternalApi.Responses;

namespace Synthesis.InProductTrainingService.Modules
{
    public sealed class InProductTrainingModule : SynthesisModule
    {
        private readonly IInProductTrainingController _inProductTrainingController;
        private const string BaseInProductTrainingUrl = "/v1/inproducttraining";

        public InProductTrainingModule(
            IInProductTrainingController inProductTrainingController,
            IMetadataRegistry metadataRegistry,
            IPolicyEvaluator policyEvaluator,
            ILoggerFactory loggerFactory)
            : base(InProductTrainingServiceBootstrapper.ServiceNameShort, metadataRegistry, policyEvaluator, loggerFactory)
        {
            _inProductTrainingController = inProductTrainingController;

            this.RequiresAuthentication();

            CreateRoute("CreateInProductTrainingView", HttpMethod.Post, $"{BaseInProductTrainingUrl}/viewed", CreateInProductTrainingViewAsync)
                .Description("Create a new InProductTraining resource")
                .StatusCodes(HttpStatusCode.Created, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError)
                .RequestFormat(InProductTrainingViewRequest.Example())
                .ResponseFormat(InProductTrainingViewResponse.Example());

            CreateRoute("GetViewedInProductTraining", HttpMethod.Get, $"{BaseInProductTrainingUrl}/viewed/{{clientApplicationId:int}}", GetViewedInProductTrainingAsync)
                .Description("Get a InProductTrainingView resource by it's identifier.")
                .StatusCodes(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound)
                .ResponseFormat(InProductTrainingViewResponse.Example());
        }

        private async Task<object> CreateInProductTrainingViewAsync(dynamic input)
        {
            InProductTrainingViewRequest newInProductTrainingViewRequest;
            try
            {
                newInProductTrainingViewRequest = this.Bind<InProductTrainingViewRequest>();
            }
            catch (Exception ex)
            {
                Logger.Error("Binding failed while attempting to create a InProductTrainingView resource", ex);
                return new InProductTrainingViewResponse { ReturnCode = CreateInProductTrainingViewReturnCode.CreateFailed };
            }

            await RequiresAccess().ExecuteAsync(CancellationToken.None);

            try
            {
                return await _inProductTrainingController.CreateInProductTrainingViewAsync(newInProductTrainingViewRequest, TenantId);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to create inProductTraining resource due to an error", ex);
                return new InProductTrainingViewResponse { ReturnCode = CreateInProductTrainingViewReturnCode.CreateFailed };
            }
        }

        private async Task<object> GetViewedInProductTrainingAsync(dynamic input)
        {
            await RequiresAccess().ExecuteAsync(CancellationToken.None);

            try
            {
                return await _inProductTrainingController.GetViewedInProductTrainingAsync(input.clientApplicationId);
            }
            catch (NotFoundException ex)
            {
                Logger.Error($"Could not find an InProductTrainingView for clientApplicationId '{input.clientApplicationId}'", ex);
                return new List<InProductTrainingViewResponse> { new InProductTrainingViewResponse { ReturnCode = CreateInProductTrainingViewReturnCode.CreateFailed } };
            }
            catch (ValidationFailedException ex)
            {
                Logger.Error($"Validation failed while attempting to get an InProductTrainingView for clientApplicationId '{input.clientApplicationId}'", ex);
                return new List<InProductTrainingViewResponse> { new InProductTrainingViewResponse { ReturnCode = CreateInProductTrainingViewReturnCode.CreateFailed } };
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get an InProductTrainingView for clientApplicationId '{input.clientApplicationId}'", ex);
                return new List<InProductTrainingViewResponse> { new InProductTrainingViewResponse { ReturnCode = CreateInProductTrainingViewReturnCode.CreateFailed } };
            }
        }
    }
}