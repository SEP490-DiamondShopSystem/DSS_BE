using DiamondShop.Domain.Common;
using DiamondShop.Domain.Models.DeliveryFees.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Models.DeliveryFees
{
    public class DeliveryFee : Entity<DeliveryFeeId>
    {
        public string DeliveryMethod { get; set; }
        public string Name { get; set; }
        public decimal Cost { get; set; }
        public int? FromKm { get; set; }
        public int? ToKm { get; set; }
        public string? FromLocation { get; set; }// cho truong hop ko co api tinh gia thi moi cai location ra
        public string? ToLocation { get; set; }
        [NotMapped]
        public bool IsDistancePriceType { get => FromKm.HasValue && ToKm.HasValue; }
        private DeliveryFee()
        {
        }
    }
}
