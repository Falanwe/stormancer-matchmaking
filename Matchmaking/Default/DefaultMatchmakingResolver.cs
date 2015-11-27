using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Matchmaking.Default
{
    public class DefaultMatchmakingResolver : IMatchmakingResolver<string, List<string>, string>
    {
        #region IConfigurationRefresh
        public void Init(dynamic config)
        {
        }

        public void ConfigChanged(dynamic newConfig)
        {
        }
        #endregion

        public Task ResolveFailure(IMatchmakingContext<string, List<string>, string> failureContext)
        {
            failureContext.Request.SendValue(new List<string>());
            return Task.FromResult(true);
        }

        public Task ResolveSuccess(IMatchmakingContext<string, List<string>, string> successContext)
        {
            successContext.Request.SendValue(successContext.MatchFoundData);
            return Task.FromResult(true);
        }
    }
}
