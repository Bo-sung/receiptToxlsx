namespace Ragnarok
{
    public static class UnitEntityFactory
    {
        public static class Character
        {
            public static UnitEntity CreateMultiPlayer()
            {
                return null;
            }

            public static PlayerBotEntity CreateGhostPlayer()
            {
                return null;
            }

            public static PlayerBotEntity CreateGhostPlayer(IMultiPlayerInput input)
            {
                return null;
            }
        }
    }
}