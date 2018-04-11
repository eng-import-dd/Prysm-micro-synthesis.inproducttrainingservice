﻿using System;
using System.IdentityModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Synthesis.InProductTrainingService.Constants;
using Synthesis.Logging;
using Synthesis.Nancy.MicroService;
using Synthesis.Nancy.MicroService.Metadata;
using Synthesis.Nancy.MicroService.Modules;
using Synthesis.Nancy.MicroService.Validation;
using Synthesis.PolicyEvaluator;
using Synthesis.InProductTrainingService.Controllers;
using Synthesis.InProductTrainingService.InternalApi.Enums;
using Synthesis.InProductTrainingService.InternalApi.Models;
using Synthesis.InProductTrainingService.InternalApi.Requests;
using Synthesis.InProductTrainingService.InternalApi.Responses;
using Synthesis.Nancy.MicroService.Constants;

namespace Synthesis.InProductTrainingService.Modules
{
    public sealed class InProductTrainingModule : SynthesisModule
    {
        private readonly IInProductTrainingController _inProductTrainingController;
        private const string BaseInProductTrainingUrl = "/v1/inproducttraining";
        private const string BaseWizardsUrl = "/v1/wizards";

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

            CreateRoute("CreateWizardView", HttpMethod.Post, $"{BaseWizardsUrl}/viewed", CreateWizardViewAsync)
                .Description("Create a new wizard view resource")
                .StatusCodes(HttpStatusCode.Created, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError)
                .RequestFormat(InProductTrainingViewRequest.Example())
                .ResponseFormat(InProductTrainingViewResponse.Example());

            CreateRoute("GetWizardViewsByUserId", HttpMethod.Get, $"{BaseWizardsUrl}/viewed/{{userId:guid}}", GetWizardViewsByUserIdAsync)
                .Description("Get a wizard view resource by it's identifier.")
                .StatusCodes(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound)
                .ResponseFormat(InProductTrainingViewResponse.Example());
        }

        private async Task<object> CreateInProductTrainingViewAsync(dynamic input)
        {
            InProductTrainingViewRequest newInProductTrainingViewRequest;
            string errorMessage;

            try
            {
                newInProductTrainingViewRequest = this.Bind<InProductTrainingViewRequest>();
            }
            catch (Exception ex)
            {
                errorMessage = "Binding failed while attempting to create a InProductTrainingView resource";
                Logger.Error(errorMessage, ex);
                return Response.BadRequestBindingException(errorMessage, ex.Message);
            }

            await RequiresAccess().ExecuteAsync(CancellationToken.None);

            try
            {
                return await _inProductTrainingController.CreateInProductTrainingViewAsync(newInProductTrainingViewRequest, PrincipalId);
            }
            catch (NotFoundException ex)
            {
                errorMessage = "Requested InProductTraining resource could not be found.";
                return Response.NotFound(ResponseReasons.NotFoundInProductTraining, errorMessage, ex.Message);
            }
            catch (ValidationFailedException ex)
            {
                errorMessage = "The InProductTraining payload is invalid.";
                return Response.BadRequestValidationException(ResponseText.BadRequestValidationFailed, errorMessage, ex.Message);
            }
            catch (RequestFailedException ex)
            {
                errorMessage = "InProductTraining resource could not be created.";
                return Response.InternalServerError(ResponseReasons.InternalServerErrorGetInProductTraining, errorMessage, ex.Message);
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to create InProductTraining resource due to an error.";
                return Response.InternalServerError(ResponseReasons.InternalServerErrorGetInProductTraining, errorMessage, ex.Message);
            }
        }

        private async Task<object> GetViewedInProductTrainingAsync(dynamic input)
        {
            await RequiresAccess().ExecuteAsync(CancellationToken.None);

            string errorMessage;

            try
            {
                return await _inProductTrainingController.GetViewedInProductTrainingAsync(input.clientApplicationId, PrincipalId);
            }
            catch (NotFoundException ex)
            {
                errorMessage = $"Could not find an InProductTrainingView for clientApplicationId '{input.clientApplicationId}'";
                return Response.NotFound(ResponseReasons.NotFoundInProductTraining, errorMessage, ex.Message);
            }
            catch (ValidationFailedException ex)
            {
                errorMessage = $"Validation failed while attempting to get an InProductTrainingView for clientApplicationId '{input.clientApplicationId}'";
                return Response.BadRequestValidationException(ResponseText.BadRequestValidationFailed, errorMessage, ex.Message);
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to get an InProductTrainingView for clientApplicationId '{input.clientApplicationId}'";
                return Response.InternalServerError(ResponseReasons.InternalServerErrorGetInProductTraining, errorMessage, ex.Message);
            }
        }

        private async Task<object> CreateWizardViewAsync(dynamic input)
        {
            WizardView newWizardView;
            string errorMessage;

            try
            {
                newWizardView = this.Bind<WizardView>();
            }
            catch (Exception ex)
            {
                errorMessage = "Binding failed while attempting to create a WizardView resource";
                Logger.Error(errorMessage, ex);

                return Response.BadRequestBindingException(errorMessage, ex.Message);
            }

            await RequiresAccess().ExecuteAsync(CancellationToken.None);

            try
            {
                return await _inProductTrainingController.CreateWizardViewAsync(newWizardView);
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to create WizardView resource due to an error.";
                return Response.InternalServerError(ResponseReasons.InternalServerErrorGetInProductTraining, errorMessage, ex.Message);
            }
        }

        private async Task<object> GetWizardViewsByUserIdAsync(dynamic input)
        {
            await RequiresAccess().ExecuteAsync(CancellationToken.None);

            string errorMessage;

            try
            {
                return await _inProductTrainingController.GetWizardViewsByUserIdAsync(input.userId);
            }
            catch (NotFoundException ex)
            {
                errorMessage = $"Could not find a WizardView for userId '{input.userId}'";
                return Response.NotFound(ResponseReasons.NotFoundInProductTraining, errorMessage, ex.Message);
            }
            catch (ValidationFailedException ex)
            {
                errorMessage = $"Validation failed while attempting to get a WizardView for userId '{input.userId}'";
                return Response.BadRequestValidationException(ResponseText.BadRequestValidationFailed, errorMessage, ex.Message);
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to get an WizardView for userId '{input.userId}'";
                return Response.InternalServerError(ResponseReasons.InternalServerErrorGetInProductTraining, errorMessage, ex.Message);
            }
        }
    }
}