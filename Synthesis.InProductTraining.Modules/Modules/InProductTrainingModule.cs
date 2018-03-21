using System;
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
using Synthesis.InProductTrainingService.Constants;
using Synthesis.InProductTrainingService.Controllers;
using Synthesis.InProductTrainingService.Models;

namespace Synthesis.InProductTrainingService.Modules
{
    public sealed class InProductTrainingModule : SynthesisModule
    {
        private readonly IInProductTrainingController _inProductTrainingController;

        public InProductTrainingModule(
            IInProductTrainingController inProductTrainingController,
            IMetadataRegistry metadataRegistry,
            IPolicyEvaluator policyEvaluator,
            ILoggerFactory loggerFactory)
            : base(InProductTrainingServiceBootstrapper.ServiceNameShort, metadataRegistry, policyEvaluator, loggerFactory)
        {
            _inProductTrainingController = inProductTrainingController;

            this.RequiresAuthentication();

            // Initialize routes
            CreateRoute("CreateInProductTrainingView", HttpMethod.Post, "/v1/inProductTraining/viewed", CreateInProductTrainingViewAsync)
                .Description("Create a new InProductTraining viewed resource")
                .StatusCodes(HttpStatusCode.Created, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError)
                .RequestFormat(InProductTraining.Example())
                .ResponseFormat(InProductTraining.Example());

            CreateRoute("GetViewedInProductTraining", HttpMethod.Get, $"/v1/inProductTraining/viewed/{{clientApplicationId}}", GetViewedInProductTrainingAsync)
                .Description("Get an InProductTraining viewed resource by an associated clientApplicationId.")
                .StatusCodes(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound)
                .ResponseFormat(InProductTraining.Example());
        }

        private async Task<object> CreateInProductTrainingViewAsync(dynamic input)
        {
            InProductTraining newInProductTraining;
            try
            {
                newInProductTraining = this.Bind<InProductTraining>();
            }
            catch (Exception ex)
            {
                Logger.Error("Binding failed while attempting to create a InProductTraining resource", ex);
                return Response.BadRequestBindingException();
            }

            // Make sure this is called outside of our typical try/catch blocks because this will
            // throw a special Nancy exception that will result in a 401 or 403 status code.
            // If we were to put this in our try/catch below, it would come across as a 500
            // instead, which is inaccurate.
            await RequiresAccess()
                .WithProjectIdExpansion(ctx => newInProductTraining.ProjectId)
                .ExecuteAsync(CancellationToken.None);

            try
            {
                var result = await _inProductTrainingController.CreateInProductTrainingViewAsync(newInProductTraining);
                return Negotiate
                    .WithModel(result)
                    .WithStatusCode(HttpStatusCode.Created);
            }
            catch (ValidationFailedException ex)
            {
                Logger.Error("Validation failed while attempting to create a InProductTraining resource", ex);
                return Response.BadRequestValidationFailed(ex.Errors);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to create inProductTraining resource due to an error", ex);
                return Response.InternalServerError(ResponseReasons.InternalServerErrorCreateInProductTraining);
            }
        }

        private async Task<object> GetViewedInProductTrainingAsync(dynamic input)
        {
            Guid id = input.id;
            InProductTraining result;

            try
            {
                result = await _inProductTrainingController.GetViewedInProductTrainingAsync(id);
            }
            catch (NotFoundException)
            {
                Logger.Error($"Could not find a '{id}'");
                return Response.NotFound(ResponseReasons.NotFoundInProductTraining);
            }
            catch (ValidationFailedException ex)
            {
                Logger.Error($"Validation failed while attempting to get '{id}'", ex);
                return Response.BadRequestValidationFailed(ex.Errors);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get '{id}'", ex);
                return Response.InternalServerError(ResponseReasons.InternalServerErrorGetInProductTraining);
            }

            // As an optimization we're getting the resource from the database first so we can use
            // the ProjectId from the resource in the expansion below (with out re-getting it).
            await RequiresAccess()
                .WithProjectIdExpansion(ctx => result.ProjectId)
                .ExecuteAsync(CancellationToken.None);

            return result;
        }
    }
}