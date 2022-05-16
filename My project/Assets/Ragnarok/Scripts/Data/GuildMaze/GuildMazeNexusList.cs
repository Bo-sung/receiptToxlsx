using UnityEngine;

namespace Ragnarok
{
    public class GuildMazeNexusList : BetterList<GuildMazeNexusInfo>
    {
        public int Size => size;


        public void Add(int teamIndex, int hp, byte state, short xIndex, short zIndex)
        {
            GuildMazeNexusInfo info = new GuildMazeNexusInfo();
            info.Initialize(teamIndex, hp, state, xIndex, zIndex);

            Add(info);
        }

        public GuildMazeNexusInfo GetNexusInfo(int teamIndex)
        {
            if (size <= teamIndex)
                return null;

            return buffer[teamIndex];
        }
    }
}