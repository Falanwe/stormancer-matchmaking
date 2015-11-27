using System.Threading.Tasks;
using Stormancer.Plugins;
using Stormancer.Configuration;

namespace Stormancer.Matchmaking
{
    public interface IMatchmakingDataExtractor<TData> : IConfigurationRefresh
    {
        Task<TData> ExtractData(RequestContext<IScenePeerClient> request);
    }
}