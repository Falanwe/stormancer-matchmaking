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
        private readonly IMatchmakingFactory _factory;
        private IMatchmakingService _matchmakingService;

        public MatchmakingPlugin(IMatchmakingFactory factory)
        {
            _factory = factory;
        }

        public void Build(HostPluginBuildContext ctx)
        {
            ctx.HostStarted += HostStarted;
            ctx.HostShuttingDown += HostShuttingDown;
        }

        private void HostStarted(IHost host)
        {
            host.AddSceneTemplate("matchmaker-template", matchmakingScene =>
            {
                if (_matchmakingService == null)
                {
                    throw new InvalidOperationException("There may only be one matchamking scene.");
                }

                _matchmakingService = _factory.CreateMatchmakingService();
                _matchmakingService.Init(matchmakingScene);


                matchmakingScene.AddProcedure("matchmaking.request", _matchmakingService.FindMatch);

            });
        }

        private void HostShuttingDown(IHost host)
        {
            var _ = this._matchmakingService.Stop();
        }
    }
}
