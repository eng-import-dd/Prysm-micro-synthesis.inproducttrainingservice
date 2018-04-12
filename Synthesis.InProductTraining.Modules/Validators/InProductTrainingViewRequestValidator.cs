using FluentValidation;
using Synthesis.InProductTrainingService.InternalApi.Requests;

namespace Synthesis.InProductTrainingService.Validators
{
    public class InProductTrainingViewRequestValidator : AbstractValidator<InProductTrainingViewRequest>
    {
        public InProductTrainingViewRequestValidator()
        {
            //RuleFor(request => request.ClientApplicationId)
            //    .NotEmpty().WithMessage("The ClientApplicationId property must not be empty");

            //RuleFor(request => request.InProductTrainingSubjectId)
            //    .NotEmpty().WithMessage("The InProductTrainingSubjectId property must not be empty");

            //RuleFor(request => request.UserId)
            //    .NotEmpty().WithMessage("The UserId property must not be empty");

            //RuleFor(request => request.UserTypeId)
            //    .NotEmpty().WithMessage("The UserTypeId property must not be empty");
        }
    }
}