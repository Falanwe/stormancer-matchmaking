using Stormancer.Core;
using Stormancer.Diagnostics;
using Stormancer.Management.Client;
using Stormancer.Server.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stormancer.Matchmaking
{
    public class MatchmakingService
    {
        private readonly ISceneHost _matchmakingScene;

        private ApplicationClient _applicationManagementClient;

        private readonly ConcurrentDictionary<IScenePeerClient, TaskCompletionSource<SceneResponse>> _waitingClients = new ConcurrentDictionary<IScenePeerClient, TaskCompletionSource<SceneResponse>>();
        private Task _runningMatch;
        private bool _isRunning= false;

        private ulong _currentIndex = 0;

        private Dictionary<ulong, DateTime> _scenesCreationTimes = new Dictionary<ulong, DateTime>();

        private TimeSpan Period
        {
            get
            {
                return TimeSpan.FromSeconds(this._matchmakingScene.GetComponent<IEnvironment>().Configuration.matchmaking.interval);
            }
        }

        public MatchmakingService(ISceneHost matchmakingScene)
        {
            this._matchmakingScene = matchmakingScene;
        }

        public async Task Init()
        {
            var environment = this._matchmakingScene.GetComponent<IEnvironment>();

            var config = environment.Configuration.matchmaking;

            this._applicationManagementClient = ApplicationClient.ForApi(config.account, config.application, config.key);

            var matchmaker = await this._applicationManagementClient.GetScene("matchmaker");
            if (matchmaker == null)
            {
                await this._applicationManagementClient.CreateScene("matchmaker", "matchmaker-template");
            }

            this._isRunning = true;
            this._runningMatch = this.Match();
        }

        public Task<SceneResponse> GetSceneForClient(IScenePeerClient scenePeerClient, CancellationToken requestCancellation)
        {
            var tcs = new TaskCompletionSource<SceneResponse>();

            requestCancellation.Register(() =>
                {
                    TaskCompletionSource<SceneResponse> _;
                    this._waitingClients.TryRemove(scenePeerClient, out _);
                    tcs.TrySetCanceled();
                });

            this._waitingClients[scenePeerClient] = tcs;

            return tcs.Task;
        }
     

        private async Task Match()
        {
            while(this._isRunning)
            { 

            await this.MatchOnce();
            await Task.Delay(this.Period);
            }
        }

        private async Task MatchOnce()
        {
            var environment = this._matchmakingScene.GetComponent<IEnvironment>();

            var config = environment.Configuration.matchmaking;

            int playersPerGame = config.maxplayers;
            while (this._waitingClients.Count >= playersPerGame)
            {
                TaskCompletionSource<SceneResponse> _;
                var players = this._waitingClients.Where(kvp => this._waitingClients.TryRemove(kvp.Key, out _)).Take(playersPerGame).ToList();


                if (players.Count < playersPerGame)
                {
                    foreach (var kvp in players)
                    {
                        this._waitingClients[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    var sceneId = (string)config.scenetype + this._currentIndex;
                    await this._applicationManagementClient.CreateScene(sceneId, config.scenetype, isPublic : false, isPersistent : false);

                    this._scenesCreationTimes.Add(this._currentIndex, DateTime.UtcNow);

                    await Task.WhenAll(players.Select(async kvp =>
                    {
                        var connectionToken = await this._applicationManagementClient.CreateConnectionToken(sceneId, kvp.Key.GetUserData<string>());
                        kvp.Value.SetResult(new SceneResponse { SceneId = sceneId, ConnectionToken = connectionToken });
                    }));

                    this._currentIndex++;
                }
            }
        }

        private async Task Stop()
        {
            this._isRunning = false;
            try
            {
                await this._runningMatch;
            }
            catch (Exception ex)
            {
                var logger = this._matchmakingScene.GetComponent<ILogger>();
                logger.Log(LogLevel.Error, "matchmaker", "Matchmaker encountered an exception.", new { exception = ex });
                
            }
        }
    }
}
