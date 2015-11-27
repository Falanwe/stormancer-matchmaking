using Stormancer.Configuration;
using Stormancer.Core;
using Stormancer.Diagnostics;
using Stormancer.Management.Client;
using Stormancer.Plugins;
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
    public static class MatchmakingService
    {
        public static IMatchmakingService Create<TData, TMatchFound, TRejection>(
            IMatchmakingDataExtractor<TData> extractor,
            IMatchmaker<TData, TMatchFound, TRejection> matchmaker,
            IMatchmakingResolver<TData, TMatchFound, TRejection> resolver)
        {
            return new MatchmakingServiceImpl<TData, TMatchFound, TRejection>(extractor, matchmaker, resolver);
        }

        private class MatchmakingServiceImpl<TData, TMachFound, TRejection> : IMatchmakingService
        {
            private ISceneHost _matchmakingScene;

            private readonly IMatchmakingDataExtractor<TData> _extractor;
            private readonly IMatchmaker<TData, TMachFound, TRejection> _matchmaker;
            private readonly IMatchmakingResolver<TData, TMachFound, TRejection> _resolver;

            //private ApplicationClient _applicationManagementClient;

            private readonly ConcurrentDictionary<IMatchmakingContext<TData, TMachFound, TRejection>, System.Reactive.Unit> _waitingClients = new ConcurrentDictionary<IMatchmakingContext<TData, TMachFound, TRejection>, System.Reactive.Unit>();


            private Task _runningMatch;
            private bool _isRunning = false;

            //private ulong _currentIndex = 0;

            private TimeSpan Period
            {
                get
                {
                    return TimeSpan.FromSeconds((double)(this._matchmakingScene.GetComponent<IEnvironment>().Configuration.matchmaking.interval));
                }
            }


            internal MatchmakingServiceImpl(IMatchmakingDataExtractor<TData> extractor, IMatchmaker<TData, TMachFound, TRejection> matchmaker, IMatchmakingResolver<TData, TMachFound, TRejection> resolver)
            {
                this._extractor = extractor;
                this._matchmaker = matchmaker;
                this._resolver = resolver;
            }

            public async Task Init(ISceneHost matchmakingScene)
            {
                var configService = this._matchmakingScene.GetComponent<ConfigurationService>();

                configService.RegisterComponent(_extractor);
                configService.RegisterComponent(_matchmaker);
                configService.RegisterComponent(_resolver);

                if (this._matchmakingScene != null)
                {
                    throw new InvalidOperationException("The matchmaking service may only be initialized once.");
                }

                this._matchmakingScene = matchmakingScene;

                this._isRunning = true;
                this._runningMatch = this.Match();
            }

            public async Task FindMatch(RequestContext<IScenePeerClient> request)
            {
                var matchmakingData = await _extractor.ExtractData(request);

                var tcs = new TaskCompletionSource<bool>();
                var context = new MatchmakingContext(request, tcs, matchmakingData);

                this._waitingClients[context] = System.Reactive.Unit.Default;

                request.CancellationToken.Register(() =>
                {
                    System.Reactive.Unit _;
                    _waitingClients.TryRemove(context, out _);

                    tcs.SetCanceled();
                });

                bool success = false;
                try
                {
                    success = await tcs.Task;
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                if (success)
                {
                    await this._resolver.ResolveSuccess(context);
                }
                else
                {
                    await this._resolver.ResolveFailure(context);
                }
            }


            private async Task Match()
            {
                while (this._isRunning)
                {

                    await this.MatchOnce();
                    await Task.Delay(this.Period);
                }
            }

            private async Task MatchOnce()
            {
                var environment = this._matchmakingScene.GetComponent<IEnvironment>();

                var config = environment.Configuration.matchmaking;
                await this._matchmaker.FindMatches(this._waitingClients.Keys.Where(c => !c.IsResolved).ToList(), config);

            }

            public async Task Stop()
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

            private class MatchmakingContext : IMatchmakingContext<TData, TMachFound, TRejection>
            {
                private TaskCompletionSource<bool> _tcs;

                public MatchmakingContext(RequestContext<IScenePeerClient> request, TaskCompletionSource<bool> tcs, TData data)
                {
                    _tcs = tcs;
                    Request = request;
                    Data = data;
                    CreationTimeUTC = DateTime.UtcNow;
                }

                public DateTime CreationTimeUTC { get; }

                public TData Data { get; set; }


                public bool Rejected { get; private set; }
                public TRejection RejectionData { get; private set; }


                public bool MatchFound { get; private set; }
                public TMachFound MatchFoundData { get; private set; }


                public RequestContext<IScenePeerClient> Request { get; }

                public void Fail(TRejection failureData)
                {

                    if (IsResolved)
                    {
                        throw new InvalidOperationException("This matchmaking context has already been resolved.");
                    }
                    Rejected = true;
                    RejectionData = failureData;
                    _tcs.SetResult(false);
                }

                public void Success(TMachFound successData)
                {
                    if (IsResolved)
                    {
                        throw new InvalidOperationException("This matchmaking context has already been resolved.");
                    }
                    MatchFound = true;
                    MatchFoundData = successData;
                    _tcs.SetResult(true);
                }

                public bool IsResolved
                {
                    get
                    {
                        return MatchFound || Rejected;
                    }
                }
            }
        }
    }


}
