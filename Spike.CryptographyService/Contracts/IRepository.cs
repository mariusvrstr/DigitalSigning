using System;
using System.Collections.Generic;
using Spike.CryptographyService.Repository;

namespace Spike.CryptographyService.Contracts
{
    public interface IRepository<T> 
        where T : IEntityBase
    {
        T GetById(Guid id);
        IList<T> FindAll();
        T Add(T entity);
        T Update(Guid id, T entity);
        void Remove(Guid id);
        void Save();
    }
}
