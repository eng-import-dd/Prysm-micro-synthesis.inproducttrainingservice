using FluentValidation;
using Synthesis.InProductTrainingService.InternalApi.Models;

namespace Synthesis.InProductTrainingService.Validators
{
    public class InProductTrainingValidator : AbstractValidator<InProductTrainingView>
    {
        public InProductTrainingValidator()
        {
            RuleFor(request => request.Title)
                .NotEmpty().WithMessage("The Title property must not be empty");
        }
    }
}