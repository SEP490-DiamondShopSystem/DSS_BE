﻿using DiamondShop.Domain.Models.JewelryModels.Entities;
using DiamondShop.Domain.Repositories.JewelryModelRepo;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Sizes.Queries.GetAll
{
    public record GetAllSizeQuery() : IRequest<List<Size>>;
    internal class GetAllSizeQueryHandler : IRequestHandler<GetAllSizeQuery, List<Size>>
    {
        private readonly ISizeRepository _sizeRepository;

        public GetAllSizeQueryHandler(ISizeRepository sizeRepository)
        {
            _sizeRepository = sizeRepository;
        }
        public async Task<List<Size>> Handle(GetAllSizeQuery request, CancellationToken cancellationToken)
        {
            var query = _sizeRepository.GetQuery();
            return (query.ToList());
        }
    }
}
