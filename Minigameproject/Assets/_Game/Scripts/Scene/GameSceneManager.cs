using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{

    public TextMeshProUGUI floor;

    public TextMeshProUGUI HP;

    private void Start()
    {
        RefreshMapHud();
    }

    public void RefreshMapHud()
    {
        HP.text = $"{RunData.currentHp}/{RunData.maxHp}";
        floor.text = RunData.currentMap != null
            ? $"용사의 연대기\nEP.{RunData.currentMap.episodeNumber}"
            : "용사의 연대기\nEP.1";
    }
    public void LoadBattleScene()
    {
        SceneManager.LoadScene("BattleScene");
    }

    public void LoadMapScene()
    {
        SceneManager.LoadScene("MapScene");
    }
    public void LoadTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }




}
