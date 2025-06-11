using UnityEngine;

public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                T[] assets = Resources.LoadAll<T>("");
                if (assets == null || assets.Length < 1)
                {
                    throw new System.Exception("Could not find any instance of singleton in folder.");
                } else if (assets.Length > 1)
                {
                    Debug.LogWarning("More than one instance of singleton found.");
                }
                instance = assets[0];
            }
            return instance;
        }
    }
}
