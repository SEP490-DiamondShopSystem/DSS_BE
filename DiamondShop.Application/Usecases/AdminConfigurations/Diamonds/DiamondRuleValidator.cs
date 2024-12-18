﻿using DiamondShop.Application.Commons.Validators;
using DiamondShop.Application.Commons.Validators.ErrorMessages;
using DiamondShop.Domain.BusinessRules;
using FluentValidation;

namespace DiamondShop.Application.Usecases.AdminConfigurations.Diamonds
{
    public class DiamondRuleValidator : AbstractValidator<DiamondRule>
    {
        public DiamondRuleValidator()
        {
            RuleFor(x => x.MinPriceOffset).GreaterThanOrEqualTo(-0.95m).WithGreaterThanMessage()
                .LessThanOrEqualTo(0.95m).WithLessThanMessage()
                .ValidNumberFraction()
                .WithMessage("MinPriceOffset only contain max 2 fractional number");

            RuleFor(x => x.MaxPriceOffset)
                .ValidNumberFraction()
                .WithMessage("MaxPriceOffset only contain max 2 fractional number");

            RuleFor(x => x.BiggestSideDiamondCarat)
                .GreaterThan(0)
                .WithMessage("BiggestSideDiamondCarat must be greater than or equal to 0");

            RuleFor(x => x.SmallestMainDiamondCarat)
                .GreaterThan(0)
                .WithMessage("SmallestMainDiamondCarat must be greater than or equal to 0");

            RuleFor(x => x.MainDiamondMaxFractionalNumber)
                .NotNull().LessThanOrEqualTo(3)
                .WithMessage("MainDiamondMaxFractionalNumber must be greater than or equal to 0");

            RuleFor(x => x.AverageOffsetVeryGoodCutFromIdealCut).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("AverageOffsetVeryGoodCutFromIdealCut must be greater than or equal to -1");

            RuleFor(x => x.AverageOffsetGoodCutFromIdealCut).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("AverageOffsetGoodCutFromIdealCut must be greater than or equal to -1");

            RuleFor(x => x.AverageOffsetVeryGoodCutFromIdealCut_FANCY_SHAPE).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("AverageOffsetVeryGoodCutFromIdealCut_FANCY_SHAPE must be greater than or equal to -1");

            RuleFor(x => x.AverageOffsetGoodCutFromIdealCut_FANCY_SHAPE).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("AverageOffsetGoodCutFromIdealCut_FANCY_SHAPE must be greater than or equal to -1");

            RuleFor(x => x.PearlOffsetFromFancyShape).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("PearlOffsetFromFancyShape must be greater than or equal to -1");

            RuleFor(x => x.PrincessOffsetFromFancyShape).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("PrincessOffsetFromFancyShape must be greater than or equal to -1");

            RuleFor(x => x.CushionOffsetFromFancyShape).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("CushionOffsetFromFancyShape must be greater than or equal to -1");

            RuleFor(x => x.EmeraldOffsetFromFancyShape).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("EmeraldOffsetFromFancyShape must be greater than or equal to -1");

            RuleFor(x => x.OvalOffsetFromFancyShape).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("OvalOffsetFromFancyShape must be greater than or equal to -1");

            RuleFor(x => x.RadiantOffsetFromFancyShape).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("RadiantOffsetFromFancyShape must be greater than or equal to -1");

            RuleFor(x => x.AsscherOffsetFromFancyShape).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("AsscherOffsetFromFancyShape must be greater than or equal to -1");

            RuleFor(x => x.MarquiseOffsetFromFancyShape).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("MarquiseOffsetFromFancyShape must be greater than or equal to -1");

            RuleFor(x => x.HeartOffsetFromFancyShape).Cascade(CascadeMode.Stop)
                .GreaterThan(-1m).ValidNumberFractionBase(4)
                .WithMessage("HeartOffsetFromFancyShape must be greater than or equal to -1");
        }
    }
}
