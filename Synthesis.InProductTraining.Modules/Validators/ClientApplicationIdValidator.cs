using FluentValidation;

namespace Synthesis.InProductTrainingService.Validators
{
    public class ClientApplicationIdValidator : AbstractValidator<int>
    {
        public ClientApplicationIdValidator()
        {
            RuleFor(request => request)
                .NotEmpty().WithMessage("The ClientApplicationId property must not be empty");
        }
    }
}