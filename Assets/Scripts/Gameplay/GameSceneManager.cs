using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "GameSceneManager", menuName = "Scriptable Objects/GameSceneManager")]
public class GameSceneManager : SingletonScriptableObject<GameSceneManager>
{

    // FREE ROAM
    public string previousArea;
    public void Navigate(string nextArea)
    {
        previousArea = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(nextArea);
    }
    public void ToDungeon()
    {
        GlobalCanvasManager.Instance.FreeRoamMenuHandler.enabled = false;
        SceneManager.LoadScene("DungeonScene");
    }

    public void ToDorm()
    {
        GlobalCanvasManager.Instance.FreeRoamMenuHandler.enabled = true;
        SceneManager.LoadScene("FRDorm");
    }
}
