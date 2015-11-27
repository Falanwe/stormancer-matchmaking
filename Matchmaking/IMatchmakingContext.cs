using Stormancer.Plugins;
using System;

namespace Stormancer.Matchmaking
{
    public interface IMatchmakingContext<TData, TMatchFound, TRejection>
    {
        RequestContext<IScenePeerClient> Request { get; }

        DateTime CreationTimeUTC { get; } 

        TData Data { get; set; }

        bool MatchFound { get; }
        TMatchFound MatchFoundData { get; }

        bool Rejected { get; }
        TRejection RejectionData { get; }

        bool IsResolved { get; }

        void Success(TMatchFound successData);
        void Fail(TRejection failureData);
    }
}