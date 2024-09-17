using DiamondShop.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatvisionRemake.Domain.Common
{
    public abstract class Entity<TId> : IHasDomainEvent where TId : notnull
    {

        protected Entity(TId id) 
        {
            Id = id;
        }
        public TId Id { get; init; }
        [NotMapped]
        private readonly List<IDomainEvent> _domainEvents = new();
        [NotMapped]
        public List<IDomainEvent> DomainEvents => _domainEvents.ToList();
        public void Raise(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
        protected Entity()
        {

        }
    }
}
