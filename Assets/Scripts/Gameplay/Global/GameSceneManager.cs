using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "GameSceneManager", menuName = "Scriptable Objects/GameSceneManager")]
public class GameSceneManager : SingletonScriptableObject<GameSceneManager>
{

    private void OnEnable()
    {
        SceneManager.sceneLoaded += GameStoryManager.Instance.OnSceneChange;
    }
    // FREE ROAM
    public string previousArea;
    public string locationName;

    public void SetLocationName(string name)
    {
        locationName = name;
    }
    public void Navigate(string nextArea)
    {
        previousArea = SceneManager.GetActiveScene().name;
        GlobalCanvasManager.Instance.FadeTransition(0.5f, delegate
        {
            SceneManager.LoadScene(nextArea);
        });
    }
    public void ToDungeon()
    {
        GlobalCanvasManager.Instance.FadeTransition(1.0f, delegate
        {
            GlobalCanvasManager.Instance.FreeRoamMenuHandler.enabled = false;
            SceneManager.LoadScene("DungeonScene");
        });
    }

    public void ToDorm()
    {
        GlobalCanvasManager.Instance.FadeTransition(1.0f, delegate
        {
            GlobalCanvasManager.Instance.FreeRoamMenuHandler.enabled = true;
            SceneManager.LoadScene("FRDorm");
        }, true, false);
    }

    public void ToSavedScene(string scene)
    {
        GlobalCanvasManager.Instance.FadeTransition(1.0f, delegate
        {
            if (scene == "DungeonScene")
                GlobalCanvasManager.Instance.FreeRoamMenuHandler.enabled = false;
            else
                GlobalCanvasManager.Instance.FreeRoamMenuHandler.enabled = true;
            SceneManager.LoadScene(scene);
        });
    }

    public void ToMainMenu()
    {
        GlobalCanvasManager.Instance.FadeTransition(1.0f, delegate
        {
            GlobalCanvasManager.Instance.FreeRoamMenuHandler.enabled = false;
            SceneManager.LoadScene("MainMenuScene");
        });
    }

    public void WarpGuildHall()
    {
        var newPrompt = PromptInfo.New("Would you like to head back to the guild hall?", new string[] { "Yes", "No" }, new PromptInfo.OptionFunction[]
        {
            delegate
            {
                Navigate("FRGuildHall");
            },
            PromptInfo.NullFunction
        });
        GlobalCanvasManager.Instance.PromptHandler.Prompt(newPrompt);
    }
    public void WarpDungeonEntrance()
    {
        var newPrompt = PromptInfo.New("Would you like to go straight to the dungeon entrance?", new string[] { "Yes", "No" }, new PromptInfo.OptionFunction[]
{
            delegate
            {
                Navigate("FRDungeon");
            },
            PromptInfo.NullFunction
});
        GlobalCanvasManager.Instance.PromptHandler.Prompt(newPrompt);
    }
}
