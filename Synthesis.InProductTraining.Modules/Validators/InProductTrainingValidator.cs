using FluentValidation;
using Synthesis.InProductTrainingService.Models;

namespace Synthesis.InProductTrainingService.Validators
{
    public class InProductTrainingValidator : AbstractValidator<InProductTraining>
    {
        public InProductTrainingValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty().WithMessage("The Name property must not be empty");
        }
    }
}
