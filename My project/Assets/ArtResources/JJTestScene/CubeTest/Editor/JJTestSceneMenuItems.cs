using UnityEditor;
using UnityEngine;

public class JJTestSceneMenuItems
{
    private static float RADIUS = 25f;
    private static int DISTANCE = 100;
    private static readonly Vector3 BASE_POSITION = Vector3.zero;

    [MenuItem("라그나로크/테스트/큐브 변형")]
    private static void CubeTransform()
    {
        const int MAP_COUNT = 6; // 맵은 정육면체

        for (int i = 0; i < MAP_COUNT; i++)
        {
            string mapName = string.Format("Map{0}", i + 1); // Map GameObject 이름
            GameObject map = GameObject.Find(mapName); // 해당 GameObject 찾기

            if (map == null)
                throw new System.Exception($"Map이 존재하지 않습니다: {nameof(mapName)} = {mapName}");

            map.transform.position = GetPosision(i);
            map.transform.rotation = GetRotation(i);
        }
    }

    private static Vector3 GetPosision(int index)
    {
        float x = BASE_POSITION.x;
        float y = BASE_POSITION.y;
        float z = BASE_POSITION.z;

        switch (index)
        {
            case 0: return new Vector3(x, y + RADIUS, z);
            case 1: return new Vector3(x, y, z + RADIUS);
            case 2: return new Vector3(x + RADIUS, y, z);
            case 3: return new Vector3(x, y, z - RADIUS);
            case 4: return new Vector3(x - RADIUS, y, z);
            case 5: return new Vector3(x, y - RADIUS, z);
        }

        throw new System.ArgumentException();
    }

    private static Quaternion GetRotation(int index)
    {
        switch (index)
        {
            case 0: return Quaternion.Euler(0, 0, 0);
            case 1: return Quaternion.Euler(90, 0, 0);
            case 2: return Quaternion.Euler(90, 90, 0);
            case 3: return Quaternion.Euler(90, 90, -90);
            case 4: return Quaternion.Euler(90, -90, 0);
            case 5: return Quaternion.Euler(0, 90, -180);

        }

        throw new System.ArgumentException();
    }

    [MenuItem("라그나로크/테스트/큐브 변형 이전")]
    private static void CubeTransformBack()
    {
        const int MAP_COUNT = 6; // 맵은 정육면체

        for (int i = 0; i < MAP_COUNT; i++)
        {
            string mapName = string.Format("Map{0}", i + 1); // Map GameObject 이름
            GameObject map = GameObject.Find(mapName); // 해당 GameObject 찾기

            if (map == null)
                throw new System.Exception($"Map이 존재하지 않습니다: {nameof(mapName)} = {mapName}");

            map.transform.position = GetPosision2(i);
            map.transform.rotation = GetRotation2(i);
        }
    }

    private static Vector3 GetPosision2(int index)
    {
        float x = BASE_POSITION.x;
        float y = BASE_POSITION.y;
        float z = BASE_POSITION.z;

        switch (index)
        {
            case 0: return new Vector3(x, y, z + DISTANCE * 0);
            case 1: return new Vector3(x, y, z + DISTANCE * 1);
            case 2: return new Vector3(x, y, z + DISTANCE * 2);
            case 3: return new Vector3(x, y, z + DISTANCE * 3);
            case 4: return new Vector3(x, y, z + DISTANCE * 4);
            case 5: return new Vector3(x, y, z + DISTANCE * 5);
        }

        throw new System.ArgumentException();
    }

    private static Quaternion GetRotation2(int index)
    {
        switch (index)
        {
            case 0: return Quaternion.Euler(0, 0, 0);
            case 1: return Quaternion.Euler(0, 0, 0);
            case 2: return Quaternion.Euler(0, 0, 0);
            case 3: return Quaternion.Euler(0, 0, 0);
            case 4: return Quaternion.Euler(0, 0, 0);
            case 5: return Quaternion.Euler(0, 0, 0);
        }

        throw new System.ArgumentException();
    }

}