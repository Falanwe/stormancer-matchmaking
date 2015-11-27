using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stormancer.Plugins;

namespace Stormancer.Matchmaking.Default
{
    public class DefaultMatchmakingDataExtractor : IMatchmakingDataExtractor<string>
    {
        #region IConfigurationRefresh
        public void Init(dynamic config)
        {
        }

        public void ConfigChanged(dynamic newConfig)
        {
        }
        #endregion

        public Task<string> ExtractData(RequestContext<IScenePeerClient> request)
        {
            return Task.FromResult(request.RemotePeer.GetUserData<string>());
        }

    }
}
