using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Matchmaking
{
    public static class MatchmakingFactory
    {
        public static IMatchmakingFactory Create<TData, TMatchFound, TRejection>(
            IMatchmakingDataExtractor<TData> extractor,
            IMatchmaker<TData, TMatchFound, TRejection> matchmaker,
            IMatchmakingResolver<TData, TMatchFound, TRejection> resolver
            )
        {
            return new MatchMakingFactoryImpl<TData, TMatchFound, TRejection>(extractor, matchmaker, resolver);
        }


        private class MatchMakingFactoryImpl<TData, TMatchFound, TRejection> : IMatchmakingFactory
        {
            private IMatchmakingDataExtractor<TData> extractor;
            private IMatchmaker<TData, TMatchFound, TRejection> matchmaker;
            private IMatchmakingResolver<TData, TMatchFound, TRejection> resolver;

            public MatchMakingFactoryImpl(IMatchmakingDataExtractor<TData> extractor, IMatchmaker<TData, TMatchFound, TRejection> matchmaker, IMatchmakingResolver<TData, TMatchFound, TRejection> resolver)
            {
                this.extractor = extractor;
                this.matchmaker = matchmaker;
                this.resolver = resolver;
            }

            public IMatchmakingService CreateMatchmakingService()
            {
                return MatchmakingService.Create(extractor, matchmaker, resolver);
            }
        }
    }


}
