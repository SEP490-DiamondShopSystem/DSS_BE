﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.CustomizeRequests.Commands.Proceed.Staff
{
    public class StaffProceedCustomizeRequestCommandValidator : AbstractValidator<StaffProceedCustomizeRequestCommand>
    {
        public StaffProceedCustomizeRequestCommandValidator()
        {
        }
    }
}