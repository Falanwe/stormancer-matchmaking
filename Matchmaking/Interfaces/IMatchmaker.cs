using Stormancer.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stormancer.Matchmaking
{
    public interface IMatchmaker<TData, TMatchFound, TRejection> : IConfigurationRefresh
    {
        Task FindMatches(IEnumerable<IMatchmakingContext<TData, TMatchFound, TRejection>> candidates, dynamic matchmakingConfig);
    }
}