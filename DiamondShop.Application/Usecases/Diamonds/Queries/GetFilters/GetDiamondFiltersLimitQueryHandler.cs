﻿using DiamondShop.Application.Dtos.Responses.Diamonds;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Models.Diamonds.Enums;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Services.interfaces;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Diamonds.Queries.GetFilters
{
   

    public record GetDiamondFiltersLimitQuery() : IRequest<DiamondFilterLimitDto>;
    internal class GetDiamondFiltersLimitQueryHandler : IRequestHandler<GetDiamondFiltersLimitQuery, DiamondFilterLimitDto>
    {
        private readonly IDiamondCriteriaRepository _diamondCriteriaRepository;
        private readonly IDiamondShapeRepository _diamondShapeRepository;
        private readonly IDiamondPriceRepository _diamondPriceRepository;
        private readonly IMapper _mapper;
        private readonly IDiamondServices _diamondServices;
        private readonly IOptionsMonitor<ApplicationSettingGlobal> _optionsMonitor;

        public GetDiamondFiltersLimitQueryHandler(IDiamondCriteriaRepository diamondCriteriaRepository, IDiamondShapeRepository diamondShapeRepository, IDiamondPriceRepository diamondPriceRepository, IMapper mapper, IDiamondServices diamondServices, IOptionsMonitor<ApplicationSettingGlobal> optionsMonitor)
        {
            _diamondCriteriaRepository = diamondCriteriaRepository;
            _diamondShapeRepository = diamondShapeRepository;
            _diamondPriceRepository = diamondPriceRepository;
            _mapper = mapper;
            _diamondServices = diamondServices;
            _optionsMonitor = optionsMonitor;
        }

        public async Task<DiamondFilterLimitDto> Handle(GetDiamondFiltersLimitQuery request, CancellationToken cancellationToken)
        {
            var diamondRule  = _optionsMonitor.CurrentValue.DiamondRule;
            var filterLimitResponse = new DiamondFilterLimitDto();
            //var getGroupedCriteria = await _diamondCriteriaRepository.GroupAllAvailableCaratRange(cancellationToken);
            var getAllShapes = await _diamondShapeRepository.GetAll(cancellationToken);
            var cuts = CutHelper.GetCutList();
            var color = ColorHelper.GetColorList();
            var clarity = ClarityHelper.GetClarityList();
            var polish = PolishHelper.GetPolishList();
            var symmetry = SymmetryHelper.GetSymmetryList();
            var minCarat = (float)diamondRule.MinCaratRange; //getGroupedCriteria.Min(x => x.CaratFrom);
            var maxCarat = (float)diamondRule.MaxCaratRange;//getGroupedCriteria.Max(x => x.CaratTo);
            filterLimitResponse.Cut.Max = (int)cuts.Max();
            filterLimitResponse.Cut.Min = (int)cuts.Min();
            filterLimitResponse.Color.Max = (int)color.Max();
            filterLimitResponse.Color.Min = (int)color.Min();
            filterLimitResponse.Clarity.Max = (int)clarity.Max();
            filterLimitResponse.Clarity.Min = (int)clarity.Min();
            filterLimitResponse.Polish.Max = (int)polish.Max();
            filterLimitResponse.Polish.Min = (int)polish.Min();
            filterLimitResponse.Symmetry.Max = (int)symmetry.Max();
            filterLimitResponse.Symmetry.Min = (int)symmetry.Min();
            filterLimitResponse.Carat.Max = maxCarat;
            filterLimitResponse.Carat.Min = 0.0f;
            filterLimitResponse.Shapes = _mapper.Map<List<DiamondShapeDto>>(getAllShapes);
            return filterLimitResponse;
        }
    }
}
