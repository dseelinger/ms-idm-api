#pragma warning disable 1591
using System.Collections.Generic;
using System.Threading.Tasks;
using IdmNet.Models;

namespace IdmApi.DAL
{
    public interface IRepository
    {
        Task<IdmResource> GetById(string id, string[] attributes);
        Task<IEnumerable<IdmResource>> GetByFilter(string filter, string[] attributes);
    }
}