using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public TextMeshProUGUI floor;
    public TextMeshProUGUI addcard;

    private void Start()
    {
        if (floor   != null) floor.text   = $"현재 {RunData.currentFloor + 1}층";
        if (addcard != null) addcard.text = $"추가된 카드: {RunData.AddedCard}장";
    }

    // ── 씬 이동 ──────────────────────────────────────

    public void LoadMapScene()
    {
        SceneManager.LoadScene("MapScene");
    }

    /// <summary>
    /// 직접 전투씬으로 이동 (타이틀 등에서 테스트용).
    /// 실제 게임 흐름은 MapManager.EnterNode() → BattleScene 이동을 사용한다.
    /// </summary>
    public void LoadBattleScene()
    {
        SceneManager.LoadScene("BattleScene");
    }

    public void LoadTitleScene()
    {
        RunData.Clear();
        SceneManager.LoadScene("TitleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
