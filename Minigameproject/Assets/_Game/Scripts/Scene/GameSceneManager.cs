using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{

    public TextMeshProUGUI floor;
    public TextMeshProUGUI addcard;

    public TextMeshProUGUI HP;

    private void Start()
    {
        RefreshMapHud();
    }

    public void RefreshMapHud()
    {
        if (RunData.currentMap != null)
        {
            floor.text = $"용사의 연대기 / EP.{RunData.currentMap.episodeNumber}";
            HP.text = $"HP:{RunData.currentHp}/{RunData.maxHp}";
            addcard.text = RunData.currentMap.episodeCompleted
                ? $"{RunData.currentMap.episodeTitle} 복구 완료"
                : $"선택: {RunData.currentMap.selectedNodeIds.Count}/3";
            return;
        }
        HP.text = $"HP:{RunData.currentHp}/{RunData.maxHp}";
        floor.text = "용사의 연대기 / EP.1";
        addcard.text = "선택: 0/3";
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
