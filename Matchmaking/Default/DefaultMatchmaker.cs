using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Matchmaking.Default
{
    public class DefaultMatchmaker : IMatchmaker<string, List<string>, string>
    {
        private int _playersPerMatch;

        #region IConfigurationRefresh
        public void Init(dynamic config)
        {
            ConfigChanged(config);
        }

        public void ConfigChanged(dynamic newConfig)
        {
            _playersPerMatch = (int)(newConfig.matchmaker.playerspermatch);
        }
        #endregion

        public Task FindMatches(IEnumerable<IMatchmakingContext<string, List<string>, string>> candidates, dynamic matchmakingConfig)
        {
            var candidatesQueue = new Queue<IMatchmakingContext<string, List<string>, string>>(candidates);

            while(candidatesQueue.Count >= _playersPerMatch)
            {
                var group = new List<IMatchmakingContext<string, List<string>, string>>();

                for(var i=0; i<_playersPerMatch; i++)
                {
                    group.Add(candidatesQueue.Dequeue());
                }

                var indices = group.Select(candidate => candidate.Data).ToList();

                foreach(var candidate in group)
                {
                    candidate.Success(indices);
                }
            }

            return Task.FromResult(true);
        }
    }
}
