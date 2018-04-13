using System;
using System.Collections.Generic;
using System.Threading;
using FluentValidation.Results;
using Moq;
using Nancy;
using Synthesis.Nancy.MicroService.Constants;
using Synthesis.Nancy.MicroService.Validation;
using Synthesis.Policy.Models;
using Synthesis.PolicyEvaluator;
using Synthesis.InProductTrainingService.Constants;
using Synthesis.InProductTrainingService.Controllers;
using Synthesis.InProductTrainingService.InternalApi.Models;
using Synthesis.InProductTrainingService.InternalApi.Requests;
using Xunit;

namespace Synthesis.InProductTrainingService.Modules.Test.Modules
{
    public class InProductTrainingModuleTest : BaseModuleTests<InProductTrainingModule>
    {
        private readonly Mock<IInProductTrainingController> _inProductTrainingControllerMock = new Mock<IInProductTrainingController>();

        private readonly int _defaultClientApplicationId = 0;
        private readonly Guid _defaultUserId = Guid.NewGuid();

        /// <inheritdoc />
        protected override List<object> BrowserDependencies => new List<object> { _inProductTrainingControllerMock.Object };

        [Fact]
        public async void GetInProductTrainingViewsReturnsBadRequestWhenControllerThrowsValidationException()
        {
            _inProductTrainingControllerMock
                .Setup(x => x.GetViewedInProductTrainingAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ThrowsAsync(new ValidationFailedException(new[] { new ValidationFailure("property name", "error") }));

            var actual = await UserTokenBrowser.Get($"/v1/inproducttraining/viewed/{_defaultClientApplicationId}", BuildRequest);

            Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
            Assert.Equal(ResponseText.BadRequestValidationFailed, actual.ReasonPhrase);
        }

        [Fact]
        public async void GetInProductTrainingViewsReturnsForbiddenWhenPermissionIsDenied()
        {
            PolicyEvaluatorMock
                .Setup(x => x.EvaluateAsync(It.IsAny<PolicyEvaluationContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PermissionScope.Deny);

            var actual = await UserTokenBrowser.Get($"/v1/inproducttraining/viewed/{_defaultClientApplicationId}", BuildRequest);

            Assert.Equal(HttpStatusCode.Forbidden, actual.StatusCode);
        }

        [Fact]
        public async void GetInProductTrainingViewsReturnsInternalServerErrorWhenControllerThrowsException()
        {
            _inProductTrainingControllerMock
                .Setup(x => x.GetViewedInProductTrainingAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var actual = await UserTokenBrowser.Get($"/v1/inproducttraining/viewed/{_defaultClientApplicationId}", BuildRequest);

            Assert.Equal(HttpStatusCode.InternalServerError, actual.StatusCode);
            Assert.Equal(ResponseReasons.InternalServerErrorGetInProductTrainingViews, actual.ReasonPhrase);
        }

        [Fact]
        public async void GetInProductTrainingViewsReturnsOk()
        {
            var actual = await UserTokenBrowser.Get($"/v1/inproducttraining/viewed/{_defaultClientApplicationId}", BuildRequest);

            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
        }

        [Fact]
        public async void InProductTrainingRespondWithUnauthorizedNoBearer()
        {
            var actual = await UnauthenticatedBrowser.Get($"/v1/inproducttraining/viewed/{_defaultClientApplicationId}", BuildRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
        }

        [Fact]
        public async void CreateInProductTrainingViewReturnsBadRequestWhenControllerThrowsValidationException()
        {
            _inProductTrainingControllerMock
                .Setup(x => x.CreateInProductTrainingViewAsync(It.IsAny<InProductTrainingViewRequest>(), It.IsAny<Guid>()))
                .ThrowsAsync(new ValidationFailedException(new[] { new ValidationFailure("property name", "error") }));

            var actual = await UserTokenBrowser.Post("/v1/inproducttraining/viewed", BuildRequest);

            Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
            Assert.Equal(ResponseText.BadRequestValidationFailed, actual.ReasonPhrase);
        }

        [Fact]
        public async void CreateInProductTrainingViewReturnsForbiddenWhenPermissionIsDenied()
        {
            PolicyEvaluatorMock
                .Setup(x => x.EvaluateAsync(It.IsAny<PolicyEvaluationContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PermissionScope.Deny);

            var actual = await UserTokenBrowser.Post("/v1/inproducttraining/viewed", BuildRequest);

            Assert.Equal(HttpStatusCode.Forbidden, actual.StatusCode);
        }

        [Fact]
        public async void CreateInProductTrainingViewReturnsInternalServerErrorWhenControllerThrowsException()
        {
            _inProductTrainingControllerMock
                .Setup(x => x.CreateInProductTrainingViewAsync(It.IsAny<InProductTrainingViewRequest>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var actual = await UserTokenBrowser.Post("/v1/inproducttraining/viewed", BuildRequest);

            Assert.Equal(HttpStatusCode.InternalServerError, actual.StatusCode);
            Assert.Equal(ResponseReasons.InternalServerErrorGetInProductTrainingViews, actual.ReasonPhrase);
        }

        [Fact]
        public async void CreateInProductTrainingViewReturnsOk()
        {
            var actual = await UserTokenBrowser.Post("/v1/inproducttraining/viewed", BuildRequest);

            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
        }

        [Fact]
        public async void CreateInProductTrainingViewRespondWithUnauthorizedNoBearer()
        {
            var actual = await UnauthenticatedBrowser.Post("/v1/inproducttraining/viewed", BuildRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
        }

        [Fact]
        public async void GetWizardViewsReturnsBadRequestWhenControllerThrowsValidationException()
        {
            _inProductTrainingControllerMock
                .Setup(x => x.GetWizardViewsByUserIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new ValidationFailedException(new[] { new ValidationFailure("property name", "error") }));

            var actual = await UserTokenBrowser.Get($"/v1/wizards/viewed/{_defaultUserId}", BuildRequest);

            Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
            Assert.Equal(ResponseText.BadRequestValidationFailed, actual.ReasonPhrase);
        }

        [Fact]
        public async void GetWizardViewsReturnsForbiddenWhenPermissionIsDenied()
        {
            PolicyEvaluatorMock
                .Setup(x => x.EvaluateAsync(It.IsAny<PolicyEvaluationContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PermissionScope.Deny);

            var actual = await UserTokenBrowser.Get($"/v1/wizards/viewed/{_defaultUserId}", BuildRequest);

            Assert.Equal(HttpStatusCode.Forbidden, actual.StatusCode);
        }

        [Fact]
        public async void GetWizardViewsReturnsInternalServerErrorWhenControllerThrowsException()
        {
            _inProductTrainingControllerMock
                .Setup(x => x.GetWizardViewsByUserIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var actual = await UserTokenBrowser.Get($"/v1/wizards/viewed/{_defaultUserId}", BuildRequest);

            Assert.Equal(HttpStatusCode.InternalServerError, actual.StatusCode);
            Assert.Equal(ResponseReasons.InternalServerErrorGetWizardViews, actual.ReasonPhrase);
        }

        [Fact]
        public async void GetWizardViewsReturnsOk()
        {
            var actual = await UserTokenBrowser.Get($"/v1/wizards/viewed/{_defaultUserId}", BuildRequest);

            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
        }

        [Fact]
        public async void WizardRespondWithUnauthorizedNoBearer()
        {
            var actual = await UnauthenticatedBrowser.Get($"/v1/wizards/viewed/{_defaultUserId}", BuildRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
        }

        [Fact]
        public async void CreateWizardViewReturnsBadRequestWhenControllerThrowsValidationException()
        {
            _inProductTrainingControllerMock
                .Setup(x => x.CreateWizardViewAsync(It.IsAny<ViewedWizard>()))
                .ThrowsAsync(new ValidationFailedException(new[] { new ValidationFailure("property name", "error") }));

            var actual = await UserTokenBrowser.Post("/v1/wizards/viewed", BuildRequest);

            Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
            Assert.Equal(ResponseText.BadRequestValidationFailed, actual.ReasonPhrase);
        }

        [Fact]
        public async void CreateWizardViewReturnsForbiddenWhenPermissionIsDenied()
        {
            PolicyEvaluatorMock
                .Setup(x => x.EvaluateAsync(It.IsAny<PolicyEvaluationContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PermissionScope.Deny);

            var actual = await UserTokenBrowser.Post("/v1/wizards/viewed", BuildRequest);

            Assert.Equal(HttpStatusCode.Forbidden, actual.StatusCode);
        }

        [Fact]
        public async void CreateWizardViewReturnsInternalServerErrorWhenControllerThrowsException()
        {
            _inProductTrainingControllerMock
                .Setup(x => x.CreateWizardViewAsync(It.IsAny<ViewedWizard>()))
                .ThrowsAsync(new Exception());

            var actual = await UserTokenBrowser.Post("/v1/wizards/viewed", BuildRequest);

            Assert.Equal(HttpStatusCode.InternalServerError, actual.StatusCode);
            Assert.Equal(ResponseReasons.InternalServerErrorCreateWizardView, actual.ReasonPhrase);
        }

        [Fact]
        public async void CreateWizardViewReturnsOk()
        {
            var actual = await UserTokenBrowser.Post("/v1/wizards/viewed", BuildRequest);

            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
        }

        [Fact]
        public async void CreateWizardViewRespondWithUnauthorizedNoBearer()
        {
            var actual = await UnauthenticatedBrowser.Post("/v1/wizards/viewed", BuildRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
        }
    }
}