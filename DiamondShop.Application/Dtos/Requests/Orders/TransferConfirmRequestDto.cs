﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Dtos.Requests.Orders
{
    public record TransferConfirmRequestDto(string TransactionId, decimal Amount, string TransactionCode);
}
