﻿using DiamondShop.Domain.Models.Orders.Enum;

namespace DiamondShop.Application.Usecases.Orders.Queries.GetOrderFilter
{
    public record OrderPaging(int pageSize = 20, int start = 0, bool? IsCustomize = null, OrderStatus? Status = null, DateTime? CreatedDate = null, DateTime? ExpectedDate = null, string? Email = null);
}
