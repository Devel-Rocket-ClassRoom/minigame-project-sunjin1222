using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

public class RuntimeTMPFontApplier : MonoBehaviour
{
    private static TMP_FontAsset runtimeFontAsset;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (FindFirstObjectByType<RuntimeTMPFontApplier>() != null)
            return;

        GameObject instance = new GameObject(nameof(RuntimeTMPFontApplier));
        DontDestroyOnLoad(instance);
        instance.AddComponent<RuntimeTMPFontApplier>();
    }

    private void Awake()
    {
        EnsureRuntimeFont();
        ApplyToAllTMPTexts();
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(ApplyRepeatedly());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToAllTMPTexts();
    }

    private IEnumerator ApplyRepeatedly()
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);

        while (true)
        {
            ApplyToAllTMPTexts();
            yield return wait;
        }
    }

    private static void EnsureRuntimeFont()
    {
        if (runtimeFontAsset != null)
            return;

        runtimeFontAsset = CreateRuntimeTMPFont();

        if (runtimeFontAsset == null)
        {
            Debug.LogWarning("[RuntimeTMPFontApplier] 사용할 수 있는 한글 TMP 폰트를 만들지 못했습니다.");
            return;
        }

        runtimeFontAsset.name = "Runtime Korean TMP Font";
        runtimeFontAsset.isMultiAtlasTexturesEnabled = true;

        if (TMP_Settings.instance != null)
            TMP_Settings.defaultFontAsset = runtimeFontAsset;
    }

    private static TMP_FontAsset CreateRuntimeTMPFont()
    {
        TMP_FontAsset bundledFontAsset = CreateFontAsset(Resources.Load<Font>("Fonts/MalgunGothic"));

        if (bundledFontAsset != null)
            return bundledFontAsset;

        string[] fontNames =
        {
            "Malgun Gothic",
            "맑은 고딕",
            "Noto Sans CJK KR",
            "Noto Sans KR",
            "NanumGothic",
            "Arial Unicode MS",
            "Arial"
        };

        foreach (string fontName in fontNames)
        {
            Font font = Font.CreateDynamicFontFromOSFont(fontName, 90);

            if (font == null)
                continue;

            TMP_FontAsset fontAsset = CreateFontAsset(font);

            if (fontAsset != null)
                return fontAsset;
        }

        return null;
    }

    private static TMP_FontAsset CreateFontAsset(Font font)
    {
        if (font == null)
            return null;

        return TMP_FontAsset.CreateFontAsset(
            font,
            90,
            9,
            GlyphRenderMode.SDFAA,
            2048,
            2048,
            AtlasPopulationMode.Dynamic,
            true);
    }

    private static void ApplyToAllTMPTexts()
    {
        EnsureRuntimeFont();

        if (runtimeFontAsset == null)
            return;

        TMP_Text[] texts = FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (TMP_Text text in texts)
        {
            if (text == null || text.font == runtimeFontAsset)
                continue;

            text.font = runtimeFontAsset;
            text.SetAllDirty();
        }
    }
}
