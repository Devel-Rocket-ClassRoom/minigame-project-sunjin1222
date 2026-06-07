using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("Selected Character")]
    public CharacterData characterData;
    [SerializeField] private CharacterData defaultCharacter;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI Play;
    public Image characterImage;

    public GameObject gameModePanel;
    public GameObject characterPanel;
    public GameObject storyPanel;
    public GameObject startimage;
    public GameObject Backimage;
    public GameObject startPanal;
    public Button storyStartButton;
    public Button continueButton;

    public Image relicImage;
    public TextMeshProUGUI relicText;


    public string mapSceneName = "MapScene";

    private void Start()
    {
        if (characterData == null)
            characterData = defaultCharacter;

        RefreshStoryStartButton();
        RefreshContinueButton();
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

        if (Play != null)
            Play.text = selectedCharacter.Playdescription;

        RunData.SetCharacter(selectedCharacter);
        RefreshStoryStartButton();
    }

    public void OnGameMode()
    {
        startPanal.SetActive(false);
        gameModePanel.SetActive(true);
        startimage.SetActive(false);
        Backimage.SetActive(true);

    }

    public void OnBackGameMode()
    {
        startPanal.SetActive(true);
        gameModePanel.SetActive(false);
        startimage.SetActive(true);
        Backimage.SetActive(false);
    }

    public void OnCharacter()
    {
        gameModePanel.SetActive(false);
        characterPanel.SetActive(true);

        if (characterData == null)
            characterData = defaultCharacter;

        SelectCharacter(characterData);
    }

    public void OnBackOnCharacter()
    {
        gameModePanel.SetActive(true);
        characterPanel.SetActive(false);
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

    public void OnBackStoryr()
    {
        storyPanel.SetActive(false);
        characterPanel.SetActive(true);

        if (characterData == null)
            characterData = defaultCharacter;

        SelectCharacter(characterData);
    }



    public void StartSelectedStory()
    {
        if (RunData.currentCharacter == null)
        {
            return;
        }

        RunData.Init();
        SceneManager.LoadScene(mapSceneName);
    }

    public void ContinueSavedStory()
    {
        if (!RunSaveSystem.Load())
            return;

        SceneManager.LoadScene(mapSceneName);
    }

    public void DeleteSavedStory()
    {
        RunSaveSystem.Delete();
        RefreshContinueButton();
    }

    private void RefreshStoryStartButton()
    {
        if (storyStartButton != null)
            storyStartButton.interactable = RunData.currentCharacter != null;
    }

    private void RefreshContinueButton()
    {
        if (continueButton != null)
            continueButton.gameObject.SetActive(RunSaveSystem.HasSave());
    }

    private RelicData GetFirstStartingRelic(CharacterData selectedCharacter)
    {
        if (selectedCharacter.startingRelics == null ||
            selectedCharacter.startingRelics.Count == 0)
            return null;

        return selectedCharacter.startingRelics[0];
    }
  
}
