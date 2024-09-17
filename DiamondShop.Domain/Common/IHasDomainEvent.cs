using BeatvisionRemake.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Common
{
    public interface IHasDomainEvent
    {
        public List<IDomainEvent> DomainEvents { get; }
        void ClearDomainEvents();
        void Raise(IDomainEvent domainEvent);
    }
}
