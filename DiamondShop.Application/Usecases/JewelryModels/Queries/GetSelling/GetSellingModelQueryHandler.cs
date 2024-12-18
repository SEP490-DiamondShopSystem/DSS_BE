﻿using DiamondShop.Application.Commons.Models;
using DiamondShop.Application.Commons.Responses;
using DiamondShop.Application.Dtos.Responses.JewelryModels;
using DiamondShop.Application.Services.Interfaces.JewelryModels;
using DiamondShop.Domain.BusinessRules;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.JewelryModels;
using DiamondShop.Domain.Models.JewelryModels.Entities;
using DiamondShop.Domain.Repositories.JewelryModelRepo;
using DiamondShop.Domain.Repositories.JewelryRepo;
using DiamondShop.Domain.Repositories.PromotionsRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Options;

namespace DiamondShop.Application.Usecases.JewelryModels.Queries.GetSelling
{
    public record GetSellingModelQuery(int page = 1, string? Category = null, string? MetalId = null, decimal? MinPrice = null, decimal? MaxPrice = null, bool? IsEngravable = null) : IRequest<Result<PagingResponseDto<JewelryModelSelling>>>;
    internal class GetSellingModelQueryHandler : IRequestHandler<GetSellingModelQuery, Result<PagingResponseDto<JewelryModelSelling>>>
    {
        private readonly Dictionary<string, JewelryModelGalleryTemplate?> cachedGallery = new();
        private readonly IJewelryModelCategoryRepository _categoryRepository;
        private readonly IJewelryRepository _jewelryRepository;
        private readonly IJewelryModelRepository _jewelryModelRepository;
        private readonly IJewelryModelFileService _jewelryModelFileService;
        private readonly IDiamondServices _diamondServices;
        private readonly IOptionsMonitor<ApplicationSettingGlobal> _optionsMonitor;
        private readonly IDiscountRepository _discountRepository;

        public GetSellingModelQueryHandler(IJewelryModelCategoryRepository categoryRepository, IJewelryRepository jewelryRepository, IJewelryModelRepository jewelryModelRepository, IJewelryModelFileService jewelryModelFileService, IDiamondServices diamondServices, IOptionsMonitor<ApplicationSettingGlobal> optionsMonitor, IDiscountRepository discountRepository)
        {
            _categoryRepository = categoryRepository;
            _jewelryRepository = jewelryRepository;
            _jewelryModelRepository = jewelryModelRepository;
            _jewelryModelFileService = jewelryModelFileService;
            _diamondServices = diamondServices;
            _optionsMonitor = optionsMonitor;
            _discountRepository = discountRepository;
        }

        public async Task<Result<PagingResponseDto<JewelryModelSelling>>> Handle(GetSellingModelQuery request, CancellationToken cancellationToken)
        {
            var getActiveDiscount = await _discountRepository.GetActiveDiscount();
            var rule = _optionsMonitor.CurrentValue.FrontendDisplayConfiguration;
            request.Deconstruct(out int page, out string? Category, out string? metalId, out decimal? minPrice, out decimal? maxPrice, out bool? isEngravable);
            var query = _jewelryModelRepository.GetSellingModelQuery();
            if (!string.IsNullOrEmpty(Category))
            {
                var category = await _categoryRepository.ContainsName(Category);
                if (category == null)
                {
                    return BlankPaging();
                }
                query = _jewelryModelRepository.QueryFilter(query, p => p.CategoryId == category.Id);
            }
            if (isEngravable != null)
            {
                query = _jewelryModelRepository.QueryFilter(query, p => p.IsEngravable == isEngravable);
            }
            List<JewelryModelSelling> sellingModels = new();
            var pageIndex = await GetData(sellingModels, query, page - 1, metalId, minPrice, maxPrice, rule.ModelPerQuery, rule.MinimumItemPerPaging);
            //assign discount
            sellingModels.ForEach(p =>
            {
                p.AssignDiscount(getActiveDiscount);
            });
            return new PagingResponseDto<JewelryModelSelling>(0, pageIndex + 1, sellingModels);
        }
        private PagingResponseDto<JewelryModelSelling> BlankPaging() => new PagingResponseDto<JewelryModelSelling>(0, 0, []);
        private async Task<int> GetData(List<JewelryModelSelling> sellingModels, IQueryable<JewelryModel> query, int page, string? metalId, decimal? minPrice, decimal? maxPrice, int modelPerQuery, int minimumItemPerPage)
        {
            var models = query.Skip(modelPerQuery * page).Take(modelPerQuery).ToList();
            if (models.Count == 0)
                return page == 0 ? 0 : page - 1;
            foreach (var model in models)
            {
                var sideDiamonds = model.SideDiamonds;
                foreach (var side in sideDiamonds)
                {
                    await _diamondServices.GetSideDiamondPrice(side);
                }
                var sizeMetals = model.SizeMetals
                    .Where(p =>
                    {
                        if (metalId != null)
                            return p.MetalId.Value == metalId;
                        else return true;
                    })
                    .GroupBy(p => p.Metal)
                    .Select(p =>
                    {
                        var min = p.MinBy(k => k.Weight);
                        var max = p.MaxBy(k => k.Weight);
                        return new
                        {
                            Metal = p.Key,
                            Min = min,
                            Max = max,
                        };
                    });
                var gallery = await GetModelGallery(model);
                foreach (var sizeMetal in sizeMetals)
                {
                    //check if model has product
                    var existedJewelry = await _jewelryRepository.GetJewelry(model.Id, sizeMetal.Metal.Id);
                    if (sideDiamonds != null && sideDiamonds.Count > 0)
                    {
                        foreach (var p in sideDiamonds)
                        {
                            var existedJewelrySide = existedJewelry.Where(k =>
                                k.SideDiamond != null &&
                                k.SideDiamond.Carat == p.CaratWeight && k.SideDiamond.SettingType == p.SettingType &&
                                k.SideDiamond.Quantity == p.Quantity && k.SideDiamond.DiamondShapeId == p.ShapeId &&
                                k.SideDiamond.ColorMin == p.ColorMin && k.SideDiamond.ColorMax == p.ColorMax &&
                                k.SideDiamond.ClarityMin == p.ClarityMin && k.SideDiamond.ClarityMax == p.ClarityMax &&
                                k.SideDiamond.IsLabGrown == p.IsLabGrown
                                );
                            if (!existedJewelrySide.Any(p => p.Status == Domain.Common.Enums.ProductStatus.Active))
                                continue;
                            var key = $"Categories/{sizeMetal.Metal.Id.Value}/{p.Id.Value}";
                            gallery.Gallery.TryGetValue(key, out List<Media>? sideDiamondImages);
                            var thumbnail = sideDiamondImages?.FirstOrDefault();
                            if (sideDiamondImages != null && sideDiamondImages.Count >= 3)
                                thumbnail = sideDiamondImages[2];
                            var reviews = existedJewelry.Where(k => k.Status == Domain.Common.Enums.ProductStatus.Sold && k.Review != null
                            ).Select(p => p.Review);
                            int totalReview = reviews.Count();
                            float starRating = totalReview == 0 ? 0 : reviews.Sum(p => p.StarRating) / totalReview;

                            var sideSelling = JewelryModelSelling.CreateWithSide(
                       thumbnail, model.Name, sizeMetal.Metal.Name, starRating, totalReview,
                       model.CraftmanFee, sizeMetal.Min.Price, sizeMetal.Max.Price,
                       model.Id, sizeMetal.Metal.Id, p);

                            //Check price
                            bool flag = true;
                            if (maxPrice != null)
                                flag = flag && (sideSelling.MinPrice <= maxPrice);
                            if (minPrice != null)
                                flag = flag && (sideSelling.MaxPrice >= minPrice);
                            if (flag)
                                sellingModels.Add(sideSelling);
                        }
                    }
                    else
                    {
                        if (!existedJewelry.Any(p => p.Status == Domain.Common.Enums.ProductStatus.Active))
                            continue;
                        var images = gallery.BaseMetals.Where(p => p.MediaPath.Contains($"Metals/{sizeMetal.Metal.Id.Value}")).ToList();
                        var thumbnail = images.FirstOrDefault();
                        if (images != null && images.Count >= 3)
                            thumbnail = images[2];
                        var reviews = existedJewelry.Where(p => p.Review != null).Select(p => p.Review);
                        int totalReview = reviews.Count();
                        float starRating = totalReview == 0 ? 0 : reviews.Sum(p => p.StarRating) / totalReview;
                        var created_noside = JewelryModelSelling.CreateNoSide(
                            thumbnail, model.Name, sizeMetal.Metal.Name, starRating, totalReview,
                            model.CraftmanFee, sizeMetal.Min.Price, sizeMetal.Max.Price, model.Id, sizeMetal.Metal.Id);
                        if (maxPrice != null)
                            if (created_noside.MinPrice > maxPrice) continue;
                        if (minPrice != null)
                            if (created_noside.MaxPrice < minPrice) continue;
                        sellingModels.Add(created_noside);
                    }
                }
            }
            if (sellingModels.Count < minimumItemPerPage)
                return await GetData(sellingModels, query, page + 1, metalId, minPrice, maxPrice, modelPerQuery, minimumItemPerPage);
            else
                return page;
        }

        private async Task<JewelryModelGalleryTemplate> GetModelGallery(JewelryModel model)
        {
            cachedGallery.TryGetValue(model.Id.Value, out JewelryModelGalleryTemplate? gallery);
            if (gallery is null)
            {
                var medias = await _jewelryModelFileService.GetFolders(model);
                gallery = _jewelryModelFileService.MapPathsToCorrectGallery(model, medias);
                cachedGallery.Add(model.Id.Value, gallery);
                model.Gallery = medias;
            }
            return gallery;
        }
    }
}
