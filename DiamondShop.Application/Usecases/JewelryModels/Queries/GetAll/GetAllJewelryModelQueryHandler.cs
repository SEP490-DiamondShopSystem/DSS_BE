﻿using DiamondShop.Application.Commons.Responses;
using DiamondShop.Domain.Models.JewelryModels;
using DiamondShop.Domain.Repositories.JewelryModelRepo;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.JewelryModels.Queries.GetAll
{
    public record GetAllJewelryModelQuery(int CurrentPage, int PageSize, string? Name, string? Code, string? Category, bool? IsEngravable, int? MainDiamondQuantity = -1) : IRequest<PagingResponseDto<JewelryModel>>;
    internal class GetAllJewelryModelQueryHandler : IRequestHandler<GetAllJewelryModelQuery, PagingResponseDto<JewelryModel>>
    {
        private readonly IJewelryModelCategoryRepository _categoryRepository;
        private readonly IJewelryModelRepository _jewelryModelRepository;
        public GetAllJewelryModelQueryHandler(IJewelryModelRepository jewelryModelRepository, IJewelryModelCategoryRepository categoryRepository)
        {
            _jewelryModelRepository = jewelryModelRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task<PagingResponseDto<JewelryModel>> Handle(GetAllJewelryModelQuery request, CancellationToken token)
        {
            request.Deconstruct(out int currentPage, out int pageSize, out string? name, out string? code, out string? categoryName, out bool? isEngravable, out int? MainDiamondQuantity);
            currentPage = currentPage == 0 ? 1 : currentPage;
            pageSize = pageSize == 0 ? 20 : pageSize;
            var query = _jewelryModelRepository.GetSellingModelQuery();
            if (!string.IsNullOrEmpty(categoryName))
            {
                var category = await _categoryRepository.ContainsName(categoryName);
                if (category == null)
                {
                    return BlankPaging();
                }
                query = _jewelryModelRepository.QueryFilter(query, p => p.CategoryId == category.Id);
            }
            //if (isRhodiumFinished != null)
            //{
            //    query = _jewelryModelRepository.QueryFilter(query, p => p.IsRhodiumFinish == isRhodiumFinished);
            //}
            if (isEngravable != null)
            {
                query = _jewelryModelRepository.QueryFilter(query, p => p.IsEngravable == isEngravable);
            }
            if (!string.IsNullOrEmpty(name))
            {
                query = _jewelryModelRepository.QueryFilter(query, p => p.Name.ToUpper().Contains(name.ToUpper()));
            }
            if (!string.IsNullOrEmpty(code))
            {
                query = _jewelryModelRepository.QueryFilter(query, p => p.ModelCode.ToUpper().Contains(code.ToUpper()));
            }
            if (MainDiamondQuantity >= 0)
            {
                query = _jewelryModelRepository.QueryFilter(query, p => p.MainDiamonds.Sum(p => p.Quantity) == MainDiamondQuantity);
            }
            int maxPage = (int)Math.Ceiling((decimal)query.Count() / pageSize);
            var list = query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            return new PagingResponseDto<JewelryModel>(maxPage, currentPage, list);
        }
        private PagingResponseDto<JewelryModel> BlankPaging() => new PagingResponseDto<JewelryModel>(0, 0, []);
    }
}
