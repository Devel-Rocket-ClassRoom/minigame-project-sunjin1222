using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    private static FirebaseManager instance;
    public static FirebaseManager Instance => instance;

    private DatabaseReference databaseRef;
    private DatabaseReference userRef;

    private bool isDataLoaded;
    public bool IsDataLoaded => isDataLoaded;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async UniTaskVoid Start()
    {
        if (FirebaseInitializer.Instance == null)
        {
            Debug.LogError("[FirebaseManager] FirebaseInitializer가 없습니다.");
            return;
        }

        if (!await FirebaseInitializer.Instance.WaitForInitializationAsync())
        {
            Debug.LogError("[FirebaseManager] Firebase 초기화 실패");
            return;
        }

        databaseRef = FirebaseInitializer.Instance.Database.RootReference;
        userRef = databaseRef.Child("users");

        isDataLoaded = true;
        Debug.Log("[FirebaseManager] 초기화 완료");
    }

    public static async UniTask SaveAsync(string uid, string json)
    {
        await GetSaveReference(uid).SetValueAsync(json);
    }

    public static async UniTask<string> LoadAsync(string uid)
    {
        DataSnapshot snapshot = await GetSaveReference(uid).GetValueAsync();

        if (!snapshot.Exists)
            return null;

        return snapshot.Value as string;
    }

    public static async UniTask<bool> HasSaveAsync(string uid)
    {
        DataSnapshot snapshot = await GetSaveReference(uid).GetValueAsync();
        return snapshot.Exists;
    }

    public static async UniTask DeleteAsync(string uid)
    {
        await GetSaveReference(uid).RemoveValueAsync();
    }

    private static DatabaseReference GetSaveReference(string uid)
    {
        return FirebaseInitializer.Instance.Database
            .GetReference($"users/{uid}/saves/currentRun");
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
