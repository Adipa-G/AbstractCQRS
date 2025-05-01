using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Core.View
{
    public interface IViewWriter
    {
        Task UpdateView(IRoot root);
    }
}