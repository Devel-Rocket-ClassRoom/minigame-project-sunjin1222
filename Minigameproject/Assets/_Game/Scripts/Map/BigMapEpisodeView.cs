using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BigMapEpisodeView : MonoBehaviour
{
    private Action<int> onEpisodeSelected;
    private Button[] episodeButtons = Array.Empty<Button>();

    public void Initialize(Action<int> episodeSelected)
    {
        onEpisodeSelected = episodeSelected;
        SetupPanZoom();
        BindButtons();
        RefreshButtons();
    }

    public void Show()
    {
        RefreshButtons();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetupPanZoom()
    {
        RectTransform viewport = transform as RectTransform;

        if (viewport == null || viewport.childCount == 0)
        {
            Debug.LogWarning("[BigMapEpisodeView] BigMap 콘텐츠를 찾지 못했습니다.");
            return;
        }

        RectTransform content = viewport.GetChild(0) as RectTransform;
        BigMapPanZoom panZoom = GetComponent<BigMapPanZoom>();

        if (panZoom == null)
            panZoom = gameObject.AddComponent<BigMapPanZoom>();

        panZoom.Configure(viewport, content);
    }

    private void BindButtons()
    {
        episodeButtons = GetComponentsInChildren<Button>(true);

        foreach (Button button in episodeButtons)
        {
            if (!TryGetEpisodeNumber(button.name, out int episodeNumber))
                continue;

            int selectedEpisode = episodeNumber;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onEpisodeSelected?.Invoke(selectedEpisode));
        }
    }

    private void RefreshButtons()
    {
        foreach (Button button in episodeButtons)
        {
            if (TryGetEpisodeNumber(button.name, out int episodeNumber))
                UpdateButtonState(button, episodeNumber);
        }
    }

    private bool TryGetEpisodeNumber(string buttonName, out int episodeNumber)
    {
        episodeNumber = 0;

        return !string.IsNullOrEmpty(buttonName) &&
            buttonName.StartsWith("Ep", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(buttonName.Substring(2), out episodeNumber);
    }

    private void UpdateButtonState(Button button, int episodeNumber)
    {
        bool isCleared = RunData.IsEpisodeCleared(episodeNumber);
        bool isUnlocked = RunData.IsEpisodeUnlocked(episodeNumber);

        ColorBlock colors = button.colors;
        colors.disabledColor = isCleared
            ? new Color(0.85f, 0.72f, 0.22f, 1f)
            : new Color(0.45f, 0.45f, 0.45f, 0.65f);
        button.colors = colors;
        button.interactable = isUnlocked && !isCleared;

        TMP_Text episodeName = button.GetComponentInChildren<TMP_Text>(true);

        if (episodeName != null)
        {
            episodeName.color = isCleared
                ? new Color(0.2f, 0.8f, 0.3f, 1f)
                : isUnlocked
                    ? new Color(0.95f, 0.2f, 0.2f, 1f)
                    : new Color(0.55f, 0.55f, 0.55f, 1f);
        }
    }
}
