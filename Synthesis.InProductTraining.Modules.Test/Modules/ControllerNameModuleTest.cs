using System;
using System.Collections.Generic;
using System.Threading;
using FluentValidation.Results;
using Moq;
using Nancy;
using Synthesis.Nancy.MicroService;
using Synthesis.Nancy.MicroService.Constants;
using Synthesis.Nancy.MicroService.Validation;
using Synthesis.Policy.Models;
using Synthesis.PolicyEvaluator;
using Synthesis.InProductTrainingService.Constants;
using Synthesis.InProductTrainingService.Controllers;
using Xunit;

namespace Synthesis.InProductTrainingService.Modules.Test.Modules
{
    public class InProductTrainingModuleTest : BaseModuleTests<InProductTrainingModule>
    {
        private readonly Mock<IInProductTrainingController> _inProductTrainingControllerMock = new Mock<IInProductTrainingController>();

        /// <inheritdoc />
        protected override List<object> BrowserDependencies => new List<object> { _inProductTrainingControllerMock.Object };

        [Fact]
        public async void GetReturnsBadRequestWhenControllerThrowsValidationException()
        {
            _inProductTrainingControllerMock
                .Setup(x => x.GetInProductTrainingAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new ValidationFailedException(new[] { new ValidationFailure("property name", "error") }));

            var actual = await UserTokenBrowser.Get($"/v1/inProductTraining/{Guid.NewGuid()}", BuildRequest);

            Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
            Assert.Equal(ResponseText.BadRequestValidationFailed, actual.ReasonPhrase);
        }

        [Fact]
        public async void GetReturnsForbiddenWhenPermissionIsDenied()
        {
            PolicyEvaluatorMock
                .Setup(x => x.EvaluateAsync(It.IsAny<PolicyEvaluationContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PermissionScope.Deny);

            var actual = await UserTokenBrowser.Get($"/v1/inProductTraining/{Guid.NewGuid()}", BuildRequest);

            Assert.Equal(HttpStatusCode.Forbidden, actual.StatusCode);
        }

        [Fact]
        public async void GetReturnsInternalServerErrorWhenControllerThrowsException()
        {
            _inProductTrainingControllerMock
                .Setup(x => x.GetInProductTrainingAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var actual = await UserTokenBrowser.Get($"/v1/inProductTraining/{Guid.NewGuid()}", BuildRequest);

            Assert.Equal(HttpStatusCode.InternalServerError, actual.StatusCode);
            Assert.Equal(ResponseReasons.InternalServerErrorGetInProductTraining, actual.ReasonPhrase);
        }

        [Fact]
        public async void GetReturnsNotFoundWhenControllerThrowsNotFound()
        {
            _inProductTrainingControllerMock
                .Setup(x => x.GetInProductTrainingAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new NotFoundException(""));

            var actual = await UserTokenBrowser.Get($"/v1/inProductTraining/{Guid.NewGuid()}", BuildRequest);

            Assert.Equal(HttpStatusCode.NotFound, actual.StatusCode);
            Assert.Equal(ResponseReasons.NotFoundInProductTraining, actual.ReasonPhrase);
        }

        [Fact]
        public async void GetReturnsOk()
        {
            var actual = await UserTokenBrowser.Get($"/v1/inProductTraining/{Guid.NewGuid()}", BuildRequest);

            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
        }

        [Fact]
        public async void RespondWithUnauthorizedNoBearer()
        {
            var actual = await UnauthenticatedBrowser.Get($"/v1/inProductTraining/{Guid.NewGuid()}", BuildRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
        }
    }
}
