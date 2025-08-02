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
        });
    }

    public void WarpGuildHall()
    {
        var prompt = new PromptInfo();
        prompt.message = "Would you like to head back to the guild hall?";
        prompt.options = new string[2];
        prompt.options[0] = "Yes";
        prompt.options[1] = "No";
        GlobalCanvasManager.Instance.PromptHandler.Prompt(prompt);
        GlobalCanvasManager.Instance.PromptHandler.StartCoroutine(GuildHallCoroutine());
    }
    private IEnumerator GuildHallCoroutine()
    {
        while (GlobalCanvasManager.Instance.PromptHandler.IsInProgress())
            yield return new WaitForEndOfFrame();
        int ans = GlobalCanvasManager.Instance.PromptHandler.TakeAnswer();
        if (ans == 0)
        {
            Navigate("FRGuildHall");
        }
    }
    public void WarpDungeonEntrance()
    {
        var prompt = new PromptInfo();
        prompt.message = "Would you like to go straight to the dungeon entrance?";
        prompt.options = new string[2];
        prompt.options[0] = "Yes";
        prompt.options[1] = "No";
        GlobalCanvasManager.Instance.PromptHandler.Prompt(prompt);
        GlobalCanvasManager.Instance.PromptHandler.StartCoroutine(DungeonCoroutine());
    }
    private IEnumerator DungeonCoroutine()
    {
        while (GlobalCanvasManager.Instance.PromptHandler.IsInProgress())
            yield return new WaitForEndOfFrame();
        int ans = GlobalCanvasManager.Instance.PromptHandler.TakeAnswer();
        if (ans == 0)
        {
            Navigate("FRDungeon");
        }
    }
}
