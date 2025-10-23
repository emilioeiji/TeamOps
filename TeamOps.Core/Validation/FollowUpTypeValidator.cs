// Project: TeamOps.Core
// File: Validation/FollowUpTypeValidator.cs
using FluentValidation;
using TeamOps.Core.Entities;

namespace TeamOps.Core.Validators
{
    public class FollowUpTypeValidator : AbstractValidator<FollowUpType>
    {
        public FollowUpTypeValidator()
        {
            RuleFor(x => x.NamePt)
                .NotEmpty().WithMessage("Portuguese name is required.")
                .MaximumLength(100);

            RuleFor(x => x.NameJp)
                .NotEmpty().WithMessage("Japanese name is required.")
                .MaximumLength(100);
        }
    }
}
