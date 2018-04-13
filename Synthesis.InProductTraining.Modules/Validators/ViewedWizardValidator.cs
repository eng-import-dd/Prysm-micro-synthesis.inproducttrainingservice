using FluentValidation;
using Synthesis.InProductTrainingService.InternalApi.Models;

namespace Synthesis.InProductTrainingService.Validators
{
    public class ViewedWizardValidator : AbstractValidator<ViewedWizard>
    {
        public ViewedWizardValidator()
        {
            RuleFor(request => request.UserId)
                .NotEmpty().WithMessage("The UserId property must not be empty");

            RuleFor(request => request.WizardType)
                .NotEmpty().WithMessage("The WizardType property must not be empty");
        }
    }
}