using System;
using UnityEngine;

public class MapJsonLoader
{
    private readonly string resourceFormat;

    public MapJsonLoader(string mapJsonResourceFormat)
    {
        resourceFormat = mapJsonResourceFormat;
    }

    public MapJsonData Load(int episodeNumber)
    {
        string mapJsonResourcePath = string.Format(resourceFormat, episodeNumber);
        TextAsset mapJson = Resources.Load<TextAsset>(mapJsonResourcePath);

        if (mapJson == null)
        {
            Debug.LogWarning($"[MapJsonLoader] Resources/{mapJsonResourcePath}.json 파일을 찾지 못했습니다.");
            return CreateEmptyMap(episodeNumber);
        }

        try
        {
            MapJsonData jsonData = JsonUtility.FromJson<MapJsonData>(mapJson.text);

            if (jsonData == null || jsonData.nodes == null)
            {
                Debug.LogWarning("[MapJsonLoader] 맵 JSON 데이터가 비어 있습니다.");
                return CreateEmptyMap(episodeNumber);
            }

            return jsonData;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[MapJsonLoader] 맵 JSON 파싱 실패: {exception.Message}");
            return CreateEmptyMap(episodeNumber);
        }
    }

    private MapJsonData CreateEmptyMap(int episodeNumber)
    {
        return new MapJsonData
        {
            episodeNumber = episodeNumber,
            episodeTitle = $"EP.{episodeNumber}",
            nodes = Array.Empty<MapNodeJsonData>()
        };
    }
}
