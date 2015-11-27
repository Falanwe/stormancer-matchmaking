using System.Threading.Tasks;
using Stormancer.Plugins;
using Stormancer.Core;

namespace Stormancer.Matchmaking
{
    public interface IMatchmakingService
    {
        Task FindMatch(RequestContext<IScenePeerClient> request);
        Task Init(ISceneHost matchmakingScene);
        Task Stop();
    }
}