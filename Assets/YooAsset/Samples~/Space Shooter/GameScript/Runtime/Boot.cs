using UnityEngine;
using YooAsset;

public class Boot : MonoBehaviour
{
    /// <summary>
    /// Resource system play mode.
    /// </summary>
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    void Awake()
    {
        Debug.Log($"Resource system play mode: {PlayMode}.");
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        // Create event manager before registering listeners.
        EventManager.Instance.Create();

        // Game manager.
        GameManager.Instance.Create();
        GameManager.Instance.SetBehaviour(this);

        // Initialize asset system.
        YooAssets.Initialize();

        // Load patch window.
        var go = Resources.Load<GameObject>("PatchWindow");
        GameObject.Instantiate(go);

        // Start patch workflow.
        PatchManager.Instance.Create("DefaultPackage", PlayMode);
        PatchManager.Instance.Start();
    }
    private void Update()
    {
        PatchManager.Instance.Update();
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
        PatchManager.Instance.Destroy();
        GameManager.Instance.Destroy();
        EventManager.Instance.Destroy();
    }
}
