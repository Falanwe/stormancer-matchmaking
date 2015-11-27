using Stormancer.Configuration;
using System.Threading.Tasks;

namespace Stormancer.Matchmaking
{
    public interface IMatchmakingResolver<TData, TMachFound, TRejection> : IConfigurationRefresh
    {
        Task ResolveSuccess(IMatchmakingContext<TData, TMachFound, TRejection> successContext);
        Task ResolveFailure(IMatchmakingContext<TData, TMachFound, TRejection> failureContext);
    }
}