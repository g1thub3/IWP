using UnityEditorInternal;
using UnityEngine;

public class SingletonMonobehaviour<T> : MonoBehaviour where T : SingletonMonobehaviour<T>
{
    public static void LoadInstance()
    {
        if (instance == null)
        {
            GameObject[] assets = Resources.LoadAll<GameObject>("");
            if (assets == null || assets.Length < 1)
            {
                throw new System.Exception("Could not find any instance of prefab in folder (no prefabs in folder).");
            }
            foreach (var asset in assets)
            {
                if (asset.TryGetComponent<T>(out T found))
                {
                    instance = Instantiate(asset).GetComponent<T>();
                    DontDestroyOnLoad(instance);
                    break;
                }
            }
            if (instance == null)
            {
                throw new System.Exception("Could not find any instance of prefab in folder (prefab not found).");
            }
        }
    }
    private static T instance;
    public static T Instance
    {
        get
        {
            LoadInstance();
            return instance;
        }
    }
}
