using Cysharp.Threading.Tasks;
using Firebase.Auth;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Authmanager : MonoBehaviour
{
    private static Authmanager instance;

    public static Authmanager Instance => instance;

    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    private bool inInitialization = false;

    private bool lastNotifiedSingendIn = false;

    public FirebaseUser CurrentUser => currentUser;

    public bool IsLogedIn => currentUser != null;

    public string UserId => currentUser?.UserId??string.Empty;

    public bool IsInitializing => inInitialization;


    public event Action<bool> LogInstateChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        if (auth != null)
            auth.StateChanged -= OnAuthStateChanged;

        instance = null;
    }



    private async UniTaskVoid Start()
    {
        bool isReady =
            await FirebaseInitializer.Instance.WaitForInitializationAsync();

        if (!isReady)
            return;

        auth = FirebaseInitializer.Instance.Auth;

        auth.StateChanged += OnAuthStateChanged;
        currentUser = auth.CurrentUser;

        inInitialization = true;
        NotifyLoginState();
    }

    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        NotifyLoginState();
    }



    private void NotifyLoginState()
    {
        bool signedIn = IsLogedIn;

        if(signedIn==lastNotifiedSingendIn)
            return;

        lastNotifiedSingendIn=signedIn;
        Debug.Log(signedIn?$"[Auth]로그인 상태:{UserId}" : "[Auth]로그아웃 상태");
        LogInstateChanged?.Invoke(signedIn);
    }


    public async UniTask<(bool susccess, string error)> CreatUseWithEmailAsync(string email,string passwd)
    {

        try
        {
            Debug.Log("[Auth] 회원가입 시도...");

            AuthResult result = await auth.CreateUserWithEmailAndPasswordAsync(email,passwd);
            currentUser = result.User;
            NotifyLoginState();
            Debug.Log($"[Auth] 회원가입 성공:{currentUser.UserId}");
            SceneManager.LoadScene("TitleScene");
            return (true, null);

        }
        catch (Exception ex)
        {
            Debug.LogError($"[Auth] 회원가입 실패: {ex.Message}");

            return (false, ParseFirebaseError(ex.Message));
        }
    }
    public async UniTask<(bool susccess, string error)> SignInUserWithEmailAsync(string email,string passwd)
    {
        try
        {
            Debug.Log("[Auth] 로그인 시도...");

            AuthResult result = await auth.SignInWithEmailAndPasswordAsync(email,passwd);
            currentUser = result.User;
            Debug.Log($"[Auth] 로그인 성공:{currentUser.UserId}");
            SceneManager.LoadScene("TitleScene");
            return (true, null);

        }
        catch (Exception ex)
        {
            Debug.LogError($"[Auth] 로그인 실패: {ex.Message}");
            return (false, ParseFirebaseError(ex.Message));
        }
    }
    public void singout()
    {
        if (auth != null && currentUser != null)
        {
            Debug.Log($"[Auth] 로그아웃");
            auth.SignOut();
            currentUser = null;
            NotifyLoginState();
        }
    }



    private string ParseFirebaseError(string error)
    {
        Debug.LogWarning($"[Auth] Firebase 에러 원문: {error}");

        string lower = error.ToLowerInvariant();

        if (lower.Contains("already in use") || lower.Contains("email-already"))
        {
            return "이미 사용 중인 이메일입니다.";
        }
        if (lower.Contains("at least 6") || lower.Contains("weak") || lower.Contains("password is invalid"))
        {
            return "비밀번호는 6자 이상이어야 합니다.";
        }
        if (lower.Contains("badly formatted") || lower.Contains("invalid-email"))
        {
            return "이메일 형식이 올바르지 않습니다.";
        }
        if (lower.Contains("network"))
        {
            return "네트워크 연결을 확인해주세요.";
        }

        return "이메일 또는 비밀번호를 확인해주세요.";
    }
}
