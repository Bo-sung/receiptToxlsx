using System.Collections.Generic;

namespace Ragnarok
{
    public interface IPassDataRepoImpl
    {
        IPassData Get(int level);
        IEnumerable<IPassData> GetEnumerable();
        IPassLevel GetLevel(int totalExp);
        int GetLastPassLevel();
        int GetLastPassExp();
    }
}