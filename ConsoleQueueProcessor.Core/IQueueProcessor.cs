using System.Threading.Tasks;

namespace ConsoleQueueProcessor.Core
{
    public interface IQueueProcessor
    {
        Task ProcessAsync();
    }
}