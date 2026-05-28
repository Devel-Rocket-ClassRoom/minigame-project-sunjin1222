using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("Selected Character")]
    public CharacterData characterData;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI descriptionText;
    public Image characterImage;

    public GameObject gameModePanel;
    public GameObject characterPanel;
    public GameObject storyPanel;
    public Button storyStartButton;

    public Image relicImage;
    public TextMeshProUGUI relicText;


    public string mapSceneName = "MapScene";

    private void Start()
    {
        RefreshStoryStartButton();
    }

    public void OnFantasyWriter()
    {
        SelectCharacter(characterData);
    }

    public void SelectCharacter(CharacterData selectedCharacter)
    {
        if (selectedCharacter == null)
        {
            Debug.LogWarning("[TitleManager] 선택할 캐릭터 데이터가 없습니다.");
            return;
        }

        characterData = selectedCharacter;
        RelicData startingRelic = GetFirstStartingRelic(selectedCharacter);

        if (nameText != null)
            nameText.text = selectedCharacter.characterName;

        if (hpText != null)
            hpText.text = $"체력: {selectedCharacter.maxHp}/{selectedCharacter.maxHp}";

        if (descriptionText != null)
            descriptionText.text = selectedCharacter.description;

        if (characterImage != null)
            characterImage.sprite = selectedCharacter.image;

        if (relicImage != null)
        {
            relicImage.sprite = startingRelic != null ? startingRelic.icon : null;
            relicImage.gameObject.SetActive(startingRelic != null);
        }

        if (relicText != null)
            relicText.text = startingRelic != null ? $"{startingRelic.relicName}\n{startingRelic.description}" : "";

        RunData.SetCharacter(selectedCharacter);
        RefreshStoryStartButton();
    }

    public void OnGameMode()
    {
        gameModePanel.SetActive(true);
    }

    public void OnCharacter()
    {
        gameModePanel.SetActive(false);
        characterPanel.SetActive(true);
    }
    public void OnStory()
    {
        if (RunData.currentCharacter == null)
        {
            Debug.LogWarning("[TitleManager] 캐릭터를 먼저 선택해야 스토리를 고를 수 있습니다.");
            return;
        }

        storyPanel.SetActive(true);
        characterPanel.SetActive(false);
    }

    public void StartSelectedStory()
    {
        if (RunData.currentCharacter == null)
        {
            Debug.LogWarning("[TitleManager] 캐릭터를 먼저 선택해야 합니다.");
            return;
        }

        RunData.Init();
        SceneManager.LoadScene(mapSceneName);
    }

    private void RefreshStoryStartButton()
    {
        if (storyStartButton != null)
            storyStartButton.interactable = RunData.currentCharacter != null;
    }

    private RelicData GetFirstStartingRelic(CharacterData selectedCharacter)
    {
        if (selectedCharacter.startingRelics == null ||
            selectedCharacter.startingRelics.Count == 0)
            return null;

        return selectedCharacter.startingRelics[0];
    }
}
