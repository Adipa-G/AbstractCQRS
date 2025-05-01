using System.Threading.Tasks;

using AbstractCqrs.Core.Command;

namespace AbstractCqrs.Sample.Service.Command
{
    public class UnitOfWork : IUnitOfWork
    {
        public void Dispose()
        {
        }

        public Task Complete()
        {
            return Task.FromResult(0);
        }
    }
}