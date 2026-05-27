using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 10층 맵 그래프를 생성한다.
/// - 각 층마다 1~2개의 노드를 배치
/// - 6층 Elite 고정, 10층(index 9) Boss 고정
/// - 층별 등장 가능 노드 타입 규칙 적용
/// </summary>
public static class MapGenerator
{
    public const int TOTAL_FLOORS = 10;

    /// <summary>
    /// 10층 분량의 노드 타입 배열을 생성해서 반환한다.
    /// index 0 = 1층, index 9 = 10층(보스)
    /// </summary>
    public static MapNodeType[] Generate()
    {
        MapNodeType[] result = new MapNodeType[TOTAL_FLOORS];

        for (int i = 0; i < TOTAL_FLOORS; i++)
        {
            result[i] = PickNodeType(i);
        }

        return result;
    }

    private static MapNodeType PickNodeType(int floorIndex)
    {
        // 보스 고정 (10층)
        if (floorIndex == TOTAL_FLOORS - 1)
            return MapNodeType.Boss;

        // 엘리트 고정 (6층 = index 5)
        if (floorIndex == 5)
            return MapNodeType.EliteBattle;

        // 층별 등장 가능 타입 풀 결정
        // 1~3층(0~2): NormalBattle만
        // 4~6층(3~5): Normal + Elite + Rest
        // 7~9층(6~8): Elite + Rest
        List<MapNodeType> pool = new List<MapNodeType>();

        if (floorIndex <= 2)
        {
            pool.Add(MapNodeType.NormalBattle);
        }
        else if (floorIndex <= 5)
        {
            pool.Add(MapNodeType.NormalBattle);
            pool.Add(MapNodeType.NormalBattle); // 비율 조절 (Normal 더 자주)
            pool.Add(MapNodeType.EliteBattle);
            pool.Add(MapNodeType.Rest);
        }
        else // 6~8 (7~9층)
        {
            pool.Add(MapNodeType.EliteBattle);
            pool.Add(MapNodeType.Rest);
        }

        return pool[Random.Range(0, pool.Count)];
    }
}
