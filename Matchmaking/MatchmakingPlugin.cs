using Stormancer.Core;
using Stormancer.Plugins;
using Stormancer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Matchmaking
{
    public class MatchmakingPlugin : IHostPlugin
    {
        private MatchmakingService _matchmakingService;

        public void Build(HostPluginBuildContext ctx)
        {
            ctx.HostStarting += HostStarting;
            ctx.HostShuttingDown += HostShuttingDown;
        }

        private void HostStarting(IHost host)
        {           
            host.AddSceneTemplate("matchmaker-template", matchmakingScene =>
            {
                this._matchmakingService = new MatchmakingService(matchmakingScene);

                matchmakingScene.AddProcedure("matchmaking.requestScene", MatchmakingRequestScene);
                
            });
        }

        private async Task MatchmakingRequestScene(RequestContext<IScenePeerClient> request)
        {
            var sceneInfo = await this._matchmakingService.GetSceneForClient(request.RemotePeer, request.CancellationToken);
            request.SendValue(sceneInfo);
        }

        private void HostShuttingDown(IHost host)
        {
            var _ = this._matchmakingService.Stop();
        }
    }
}
