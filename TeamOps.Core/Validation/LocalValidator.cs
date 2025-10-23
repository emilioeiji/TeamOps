using FluentValidation;
using TeamOps.Core.Entities;

namespace TeamOps.Core.Validators
{
    public class LocalValidator : AbstractValidator<Local>
    {
        public LocalValidator()
        {
            RuleFor(x => x.NamePt)
                .NotEmpty().WithMessage("Nome em português é obrigatório.")
                .MaximumLength(100);

            RuleFor(x => x.NameJp)
                .NotEmpty().WithMessage("Nome em japonês é obrigatório.")
                .MaximumLength(100);

            RuleFor(x => x.SectorId)
                .GreaterThan(0).WithMessage("Setor é obrigatório.");
        }
    }
}
