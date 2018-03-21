using FluentValidation;
using System;

namespace Synthesis.InProductTrainingService.Validators
{
    public abstract class GuidValidator : AbstractValidator<Guid>
    {
        protected GuidValidator(string name)
        {
            RuleFor(guid => guid).NotEqual(Guid.Empty).WithMessage($"The {name} must not be empty");
        }
    }
}
