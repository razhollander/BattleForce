using System.Collections.Generic;

namespace Core.Scripts.Extensions.Linq
{
    public class LinqHashsetExtensions
    {
        public T First<T>(HashSet<T> hashSet)
        {
            foreach (var teamId in hashSet)
            {
                winningTeamId = teamId;
                break;
            }

        }
    }
}