using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
<<<<<<< HEAD
using Cysharp.Threading.Tasks;

=======
>>>>>>> origin/main

public class GameSceneManager : MonoBehaviour
{

<<<<<<< HEAD


=======
>>>>>>> origin/main
    public TextMeshProUGUI floor;

    public TextMeshProUGUI HP;

    public GameObject end;

    private void OnEnable()
    {
        RunData.HealthChanged += RefreshMapHud;
    }

    private void OnDisable()
    {
        RunData.HealthChanged -= RefreshMapHud;
    }

    private void Start()
    {
        RefreshMapHud();
    }

    public void RefreshMapHud()
    {
        if (HP != null)
            HP.text = $"{RunData.currentHp}/{RunData.maxHp}";

        if (floor != null)
        {
            floor.text = RunData.currentMap != null
                ? $"용사의 연대기\nEP.{RunData.currentMap.episodeNumber}"
                : "용사의 연대기\nEP.1";
        }
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

<<<<<<< HEAD
    public async void SaveRun()
    {
        await RunSaveSystem.SaveToFirebaseAsync();
    }

    public async void SaveAndLoadTitle()
    {
        if (await RunSaveSystem.SaveToFirebaseAsync())
            SceneManager.LoadScene("TitleScene");
    }

    public async void LoadSavedRun()
    {
        if (await RunSaveSystem.LoadFromFirebaseAsync())
            SceneManager.LoadScene("MapScene");
    }

    public async void Logout()
    {
        Authmanager.Instance.singout();
        SceneManager.LoadScene("LoginScene");
    }

    public async void DeleteSavedRun()
    {
        await RunSaveSystem.DeleteFromFirebaseAsync();
    }


=======
    public void SaveRun()
    {
        RunSaveSystem.Save();
    }

    public void LoadSavedRun()
    {
        if (RunSaveSystem.Load())
            SceneManager.LoadScene("MapScene");
    }

    public void DeleteSavedRun()
    {
        RunSaveSystem.Delete();
    }

>>>>>>> origin/main
    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnendPanal()
    {
        end.SetActive(true);
<<<<<<< HEAD
    }
=======
    } 
>>>>>>> origin/main


    public void ClendPanal()
    {
        end.SetActive(false);
<<<<<<< HEAD
    }
=======
    } 
>>>>>>> origin/main


}
