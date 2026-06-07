using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SaveAssetResolver
{
    public static CardData FindCard(string id)
    {
        return FindAsset<CardData>(id, card => card.cardId);
    }

    public static RelicData FindRelic(string id)
    {
        return FindAsset<RelicData>(id, relic => relic.relicId);
    }

    public static CharacterData FindCharacter(string id)
    {
        return FindAsset<CharacterData>(id, character => character.characterId);
    }

    public static EnemyData FindEnemy(string id)
    {
        return FindAsset<EnemyData>(id, enemy => enemy.enemyId);
    }

    public static EventData FindEvent(string id)
    {
        return FindAsset<EventData>(id, eventData => eventData.eventId);
    }

    private static T FindAsset<T>(string id, Func<T, string> idSelector)
        where T : ScriptableObject
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        T resourceAsset = FindInResources(id, idSelector);
        if (resourceAsset != null)
            return resourceAsset;

#if UNITY_EDITOR
        return FindInAssetDatabase(id, idSelector);
#else
        Debug.LogWarning($"[SaveAssetResolver] Resources에서 {typeof(T).Name} '{id}'를 찾지 못했습니다.");
        return null;
#endif
    }

    private static T FindInResources<T>(string id, Func<T, string> idSelector)
        where T : ScriptableObject
    {
        T[] assets = Resources.LoadAll<T>("");

        foreach (T asset in assets)
        {
            if (asset != null && idSelector(asset) == id)
                return asset;
        }

        return null;
    }

#if UNITY_EDITOR
    private static T FindInAssetDatabase<T>(string id, Func<T, string> idSelector)
        where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null && idSelector(asset) == id)
                return asset;
        }

        Debug.LogWarning($"[SaveAssetResolver] AssetDatabase에서 {typeof(T).Name} '{id}'를 찾지 못했습니다.");
        return null;
    }
#endif
}
