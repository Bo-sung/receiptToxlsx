using UnityEngine;

namespace Ragnarok
{
    public struct MazeMapTuple
    {
        private const int EMPTY = 0; // 길
        private const int WALL = 1; // 벽
        private const int START = 2; // 유저 시작 지점
        private const int USER_WALL = 3; // 유저만 지날 수 있는 벽
        private const int MAZE_CUBE = 4; // 큐브 조각 생성 지점
        private const int MONSTER = 5; // 몹 생성 지점 + 제니 생성 지점
        private const int ZENY = 6; // 제니 생성 지점
        private const int ITEM = 7; // 랜덤 아이템 생성 지점

        private const float MAP_SCALE = 0.65f;
        private const float CELL_SIZE = 3.6f;

        private enum CheckType
        {
            MazeCube, Zeny, Item,
        }

        private readonly static Buffer<Vector3> buffer = new Buffer<Vector3>();

        private readonly int sizeX;
        private readonly int sizeY;
        private readonly int[,] map;

        public MazeMapTuple(string data, int x, int y)
        {
            sizeX = x;
            sizeY = y;

            map = new int[sizeY, sizeX];
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    map[i, j] = GetMapValue(data[i * sizeX + j]);
                }
            }
        }

        public int GetMazeCubeCount()
        {
            return GetCount(CheckType.MazeCube);
        }

        public Vector3[] GetZenyPositions(int maxCount)
        {
            return GetPositions(CheckType.Zeny, maxCount);
        }

        public Vector3[] GetItemPositions(int maxCount)
        {
            return GetPositions(CheckType.Item, maxCount);
        }

        private int GetMapValue(char ch)
        {
            switch (ch)
            {
                case '0': return EMPTY;
                case '1': return WALL;
                case '2': return START;
                case '3': return USER_WALL;
                case '4': return MAZE_CUBE;
                case '5': return MONSTER;
                case '6': return ZENY;
                case '7': return ITEM;
            }

            return EMPTY;
        }

        private int GetCount(CheckType checkType)
        {
            int count = 0;

            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (IsCheck(map[i, j], checkType))
                        ++count;
                }
            }

            return count;
        }

        private Vector3[] GetPositions(CheckType checkType, int maxCount)
        {
            const float SIZE = CELL_SIZE * MAP_SCALE;

            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (IsCheck(map[i, j], checkType))
                    {
                        buffer.Add(new Vector3(i * SIZE, 0f, j * SIZE));
                    }
                }
            }

            return buffer.GetRandomPop(maxCount, isAutoRelease: true);
        }

        private bool IsCheck(int num, CheckType checkType)
        {
            switch (checkType)
            {
                case CheckType.MazeCube:
                    return num == MAZE_CUBE;

                case CheckType.Zeny:
                    return num == MONSTER || num == ZENY;

                case CheckType.Item:
                    return num == ITEM;
            }

            return false;
        }
    }
}