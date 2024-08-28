using DiamondShop.Application.Commons.Responses;
using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Repositories;
using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Customers.Queries.GetCustomerPage
{

    public record GetCustomerPageQuery(int current = 0, int size = 10) : IRequest<Result<PagingResponseDto<Customer>>>;
    internal class GetCustomerPageQueryHandler : IRequestHandler<GetCustomerPageQuery, Result<PagingResponseDto<Customer>>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerPageQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Result<PagingResponseDto<Customer>>> Handle(GetCustomerPageQuery request, CancellationToken cancellationToken)
        {
            var query = _customerRepository.GetQuery();
            var trueCurrent = request.current * request.size;
            query.Skip(trueCurrent).Take(request.size);
            
            var result = query.ToList();
            var totalPage = _customerRepository.GetCount();
            return Result.Ok(new PagingResponseDto<Customer>(
                totalPage : totalPage,
                currentPage : request.current,
                Values: result));
        }
    }
}
