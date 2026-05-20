using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{


    public void LoadBattleScene()
    {
        SceneManager.LoadScene("BattleScene");
    }

     public void LoadMapScene()
    {
        SceneManager.LoadScene("MapScene");
    }
}