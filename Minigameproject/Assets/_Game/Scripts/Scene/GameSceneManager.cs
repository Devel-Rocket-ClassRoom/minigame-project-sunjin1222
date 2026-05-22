using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{

    public TextMeshProUGUI floor;
    public TextMeshProUGUI addcard;

    private void Start()
    {
        floor.text = $"현재{RunData.currentFloor}층";
        addcard.text = $"추가된 카드:{RunData.AddedCard}장";
    }
    public void LoadBattleScene()
    {
        SceneManager.LoadScene("BattleScene");
    }

    public void LoadMapScene()
    {
        SceneManager.LoadScene("MapScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }




}