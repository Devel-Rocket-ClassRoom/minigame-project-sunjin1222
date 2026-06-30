using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LoginUI : MonoBehaviour
{

    [Header("Login Form")]
    [SerializeField]
    private TMP_InputField emailInput;

    [SerializeField]
    private TMP_InputField passwordInput;

    [SerializeField]
    private Button loginButton;

    [SerializeField]
    private Button signupButton;



    [SerializeField]
    private TextMeshProUGUI errorText;



    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(()=>Authmanager.Instance.IsInitializing);

    
        loginButton.onClick.AddListener(() => OnLoginButtonClicked().Forget());
        signupButton.onClick.AddListener(() => OnSignupButtonClicked().Forget());

        UpdateUI().Forget();

    }

    public async UniTaskVoid UpdateUI()
    {
        if(!Authmanager.Instance.IsInitializing)
            return;

        bool isLoggedIn = Authmanager.Instance.IsLogedIn;

        await UniTask.CompletedTask;
    }

    private async UniTaskVoid OnLoginButtonClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        { 
            ShowError("이메일과 비밀번호를 입력해주세요.");
            return;
        }

        SetButtonsInteractable(false);
        var (success, error) = await Authmanager.Instance.SignInUserWithEmailAsync(email, password);
        if (success)
        {
            UpdateUI().Forget();

        }
        else
        { 
            ShowError(error);
        }
        SetButtonsInteractable(true);


    }

    private async UniTaskVoid OnSignupButtonClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("이메일과 비밀번호를 입력해주세요.");
            return;
        }

        SetButtonsInteractable(false);

        var (success, error) =
            await Authmanager.Instance.CreatUseWithEmailAsync(email, password);

        if (success)
            UpdateUI().Forget();
        else
            ShowError(error);

        SetButtonsInteractable(true);
    }
   

    private void ShowError(string message)
    {
        errorText.text = message;
        errorText.color = Color.red;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        loginButton.interactable = interactable;
        signupButton.interactable = interactable;
        
    }
}
