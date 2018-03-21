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

            // CRUD routes
            CreateRoute("CreateInProductTraining", HttpMethod.Post, "/v1/inProductTraining", CreateInProductTrainingAsync)
                .Description("Create a new InProductTraining resource")
                .StatusCodes(HttpStatusCode.Created, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError)
                .RequestFormat(InProductTraining.Example())
                .ResponseFormat(InProductTraining.Example());

            CreateRoute("GetInProductTraining", HttpMethod.Get, "/v1/inProductTraining/{id:guid}", GetInProductTrainingAsync)
                .Description("Get a InProductTraining resource by it's identifier.")
                .StatusCodes(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound)
                .ResponseFormat(InProductTraining.Example());

            CreateRoute("UpdateInProductTraining", HttpMethod.Put, "/v1/inProductTraining/{id:guid}", UpdateInProductTrainingAsync)
                .Description("Update a InProductTraining resource.")
                .StatusCodes(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound)
                .RequestFormat(InProductTraining.Example())
                .ResponseFormat(InProductTraining.Example());

            CreateRoute("DeleteInProductTraining", HttpMethod.Delete, "/v1/inProductTraining/{id:guid}", DeleteInProductTrainingAsync)
                .Description("Delete a InProductTraining resource.")
                .StatusCodes(HttpStatusCode.NoContent, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError);
        }

        private async Task<object> CreateInProductTrainingAsync(dynamic input)
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
                var result = await _inProductTrainingController.CreateInProductTrainingAsync(newInProductTraining);
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

        private async Task<object> GetInProductTrainingAsync(dynamic input)
        {
            Guid id = input.id;
            InProductTraining result;

            try
            {
                result = await _inProductTrainingController.GetInProductTrainingAsync(id);
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

        private async Task<object> UpdateInProductTrainingAsync(dynamic input)
        {
            Guid id = input.id;
            InProductTraining inProductTrainingModel;

            try
            {
                inProductTrainingModel = this.Bind<InProductTraining>();
            }
            catch (Exception ex)
            {
                Logger.Warning("Binding failed while attempting to update a InProductTraining resource.", ex);
                return Response.BadRequestBindingException();
            }

            await RequiresAccess()
                .WithProjectIdExpansion(async (ctx, ct) =>
                {
                    var resource = await _inProductTrainingController.GetInProductTrainingAsync(id);
                    return resource.ProjectId;
                })
                .ExecuteAsync(CancellationToken.None);

            try
            {
                return await _inProductTrainingController.UpdateInProductTrainingAsync(id, inProductTrainingModel);
            }
            catch (ValidationFailedException ex)
            {
                Logger.Error($"Validation failed while attempting to update '{id}'", ex);
                return Response.BadRequestValidationFailed(ex.Errors);
            }
            catch (NotFoundException)
            {
                Logger.Error($"Could not find '{id}'");
                return Response.InternalServerError(ResponseReasons.InternalServerErrorUpdateInProductTraining);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unhandled exception encountered while attempting to update '{id}'", ex);
                return Response.InternalServerError(ResponseReasons.InternalServerErrorUpdateInProductTraining);
            }
        }

        private async Task<object> DeleteInProductTrainingAsync(dynamic input)
        {
            Guid id = input.id;

            await RequiresAccess()
                .WithProjectIdExpansion(async (ctx, ct) =>
                {
                    var resource = await _inProductTrainingController.GetInProductTrainingAsync(id);
                    return resource.ProjectId;
                })
                .ExecuteAsync(CancellationToken.None);

            try
            {
                await _inProductTrainingController.DeleteInProductTrainingAsync(id);

                return new Response
                {
                    StatusCode = HttpStatusCode.NoContent,
                    ReasonPhrase = "Resource has been deleted"
                };
            }
            catch (ValidationFailedException ex)
            {
                Logger.Error($"Validation failed while attempting to delete '{id}'", ex);
                return Response.BadRequestValidationFailed(ex.Errors);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unhandled exception encountered while attempting to delete '{id}'", ex);
                return Response.InternalServerError(ResponseReasons.InternalServerErrorDeleteInProductTraining);
            }
        }
    }
}
