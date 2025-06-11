using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition
{
    public float t;
    public float max;

    public Transition()
    {
        t = 0;
        max = 1.0f;
    }
    public float Progression
    {
        get { return t / max; }
    }
    public void Progress()
    {
        t = Mathf.Clamp(t + Time.deltaTime, 0, max);
    }
    public void Revert()
    {
        t = Mathf.Clamp(t - Time.deltaTime, 0, max);
    }
}
