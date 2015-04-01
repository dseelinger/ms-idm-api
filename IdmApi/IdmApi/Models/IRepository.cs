using System.Threading.Tasks;
using IdmNet;

namespace IdmApi.Models
{
    public interface IRepository
    {
        Task<IdmResource> GetById(string id, string[] attributes);
    }
}