using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] CanvasGroup logo;
    void Start()
    {
        GlobalCanvasManager.LoadInstance();
        logo.alpha = 0;
        StartCoroutine(Splash());
    }

    private IEnumerator Splash()
    {
        bool skip = false;
        Transition trans = new Transition();
        trans.max = 1.0f;
        while (trans.Progression < 1.0f)
        {
            trans.Progress();
            logo.alpha = trans.Progression;
            yield return new WaitForEndOfFrame();
        }

        trans.t = 0;
        trans.max = 3.0f;
        while (trans.Progression < 1.0f)
        {
            if (skip) break;
            if (Input.GetKey(KeyCode.Z))
                skip = true;
            trans.Progress();
            yield return new WaitForEndOfFrame();
        }

        GlobalCanvasManager.Instance.FadeTransition(1.0f, delegate
        {
            SceneManager.LoadScene("MainMenuScene");
        });
    }
}
