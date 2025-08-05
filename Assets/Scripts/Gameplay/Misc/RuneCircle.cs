using System.Collections;
using UnityEngine;

public class RuneCircle : MonoBehaviour, IYieldable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Transform runeSprite;
    [SerializeField] private float rotateSpeed = 50;

    bool isTweening = false;
    public bool IsInProgress()
    {
        return isTweening;
    }

    private IEnumerator ScaleCoroutine(float size, float time)
    {
        isTweening = true;
        Vector3 ogSize = transform.localScale;
        Transition newTrans = new Transition();
        newTrans.max = time;
        while (newTrans.Progression < 1)
        {
            newTrans.Progress();
            transform.localScale = Vector3.Lerp(ogSize, new Vector3(size, size, size), newTrans.Progression);
            yield return new WaitForEndOfFrame();
        }
        isTweening = false;
    }

    public void Scale(float size, float time)
    {
        StartCoroutine(ScaleCoroutine(size, time));
    }

    // Update is called once per frame
    void Update()
    {
        runeSprite.transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
}
