using System;
using FluentValidation;

namespace Synthesis.InProductTrainingService.Validators
{
    public class UserIdValidator : AbstractValidator<Guid>
    {
        public UserIdValidator()
        {
            RuleFor(request => request)
                .NotEmpty().WithMessage("The UserId property must not be empty");
        }
    }
}