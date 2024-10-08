﻿using DiamondShop.Application.Commons.Responses;
using DiamondShop.Domain.Models.Diamonds;
using DiamondShop.Domain.Models.Diamonds.Enums;
using FluentResults;
using MediatR;
namespace DiamondShop.Application.Usecases.Diamonds.Queries.GetPaging
{
    public record GetDiamond_4C(Cut? cutFrom, Cut? cutTo, Color? colorFrom, Color? colorTo, Clarity? clarityFrom, Clarity? clarityTo, float? caratFrom, float? caratTo);
    public record GetDiamond_Details(Polish? Polish, Symmetry? Symmetry, Girdle? Girdle, Fluorescence? Fluorescence, Culet? Culet, bool isGIA = true);
    public record GetDiamondPagingQuery(int pageSize = 20, int start = 0,
        GetDiamond_4C? diamond_4C = null, GetDiamond_Details? diamond_Details = null) : IRequest<Result<PagingResponseDto<Diamond>>>;
}
