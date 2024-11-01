﻿using DiamondShop.Domain.Models.Promotions.Enum;
using FluentValidation;

namespace DiamondShop.Application.Usecases.PromotionGifts.Commands.CreateMany
{
    public class CreateGiftCommandValidator : AbstractValidator<CreateGiftCommand>
    {
        public CreateGiftCommandValidator()
        {
            var validator = new GiftSpecValidator();
            RuleFor(x => x.giftSpecs).Cascade(CascadeMode.Stop).NotNull();
            RuleForEach(x => x.giftSpecs).SetValidator(validator);
            When(x => x.giftSpecs.Any(g => g.TargetType == TargetType.Order), () => 
            {
                RuleFor(x => x.giftSpecs)
                .Must(x => x.Count(g => g.TargetType == TargetType.Order) == 1)
                .WithName("OrderGiftCount")
                .WithMessage("Only one gift can be set for order.");
            });
        }
    }
    public class GiftSpecValidator : AbstractValidator<GiftSpec>
    {
        public GiftSpecValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
            RuleFor(x => x.TargetType).IsInEnum().WithMessage("TargetType is not valid.");
            RuleFor(x => x.UnitType).IsInEnum();
            RuleFor(x => x.UnitValue).GreaterThan(0);

            When(x => x.TargetType == TargetType.Diamond, () =>
            {
                RuleFor(x => x.DiamondRequirementSpec).NotNull();
                RuleFor(x => x.DiamondRequirementSpec.Origin).IsInEnum().WithMessage("not valid origin, can only be lab = 1, natural = 2, both = 3 ");

                RuleFor(x => x.DiamondRequirementSpec.caratFrom).GreaterThanOrEqualTo(0);
                RuleFor(x => x.DiamondRequirementSpec.caratTo).GreaterThanOrEqualTo(0);
                RuleFor(x => x.DiamondRequirementSpec.caratFrom).LessThanOrEqualTo(x => x.DiamondRequirementSpec.caratTo);

                RuleFor(x => (int)x.DiamondRequirementSpec.cutFrom).LessThanOrEqualTo(x => (int)x.DiamondRequirementSpec.cutTo);
                RuleFor(x => (int)x.DiamondRequirementSpec.clarityFrom).LessThanOrEqualTo(x => (int)x.DiamondRequirementSpec.clarityTo);
                RuleFor(x => (int)x.DiamondRequirementSpec.colorFrom).LessThanOrEqualTo(x => (int)x.DiamondRequirementSpec.colorTo);
            });
            When(x => x.TargetType == TargetType.Jewelry_Model, () =>
            {
                RuleFor(x => x.itemId).NotNull();
            });
        }
    }
}
