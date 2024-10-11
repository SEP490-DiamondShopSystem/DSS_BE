using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.BusinessRules
{
    public static class TransactionRules
    {
        public static long MinimumPerTransaction { get; set; } = 10000;// 10k
        public static long MaximumPerTransaction { get; set; } = 200000000;//200tr
        public static int TransactionDurationMinute { get; set; } = 15;
        /// <summary>
        /// đây là giá trị tối đa của dơn hàng mà nếu nó lớn hơn giá trị này thì bắt buộc phải 
        /// thông qua ít nhất 2 giao dịch
        /// </summary>
        public static long MaximumForOneTimeTransaction { get; set; } = 50000000;//50tr
        public static long MinimumForManyTimeTransaction { get; set; } = 25000000;//25tr
        public static string TransactionTimeStamp { get; set; } = "yyyyMMddHHmmss";
    }
}
