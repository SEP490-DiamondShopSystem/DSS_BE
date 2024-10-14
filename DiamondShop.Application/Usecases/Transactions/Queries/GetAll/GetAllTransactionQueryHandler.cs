using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Domain.Repositories.TransactionRepo;
using MediatR;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DiamondShop.Application.Usecases.Transactions.Queries.GetAll
{
    public record GetAllTransactionQuery() :IRequest<List<Transaction>>;
    internal class GetAllTransactionQueryHandler : IRequestHandler<GetAllTransactionQuery, List<Transaction>>
    {
        private readonly ITransactionRepository _transactionRepository;

        public GetAllTransactionQueryHandler(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<List<Transaction>> Handle(GetAllTransactionQuery request, CancellationToken cancellationToken)
        {
            return await _transactionRepository.GetAll();
        }
    }
}
