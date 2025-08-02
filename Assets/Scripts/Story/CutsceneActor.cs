using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CutsceneActor : MonoBehaviour, IYieldable
{
    public static List<CutsceneActor> MovingActors;
    public static string[] Directions = {"north", "east", "south", "west"};
    [Range(0, 3)]
    [SerializeField] private int _direction;
    protected Animator _animator;

    public int Direction
    {
        get { return _direction; }
    }

    private bool _moveInProgress;
    private bool _animInProgress;
    private bool _skipAction;

    public void SkipAction()
    {
        _skipAction = true;
    }

    public static void SkipMovingActors()
    {
        foreach (var actor in MovingActors)
            actor.SkipAction();
    }

    public bool IsInProgress()
    {
        return (_moveInProgress || _animInProgress);
    }

    public void MoveActor(Transform point, float speed = 1)
    {
        float dist = (transform.position - point.position).magnitude;
        float t = dist / speed;
        MovingActors.Add(this);
        _skipAction = false;
        StartCoroutine(MoveCoroutine(transform.position, point.position, t));
    }
    public void PlayAnimation(string name)
    {
        _animInProgress = true;
        _animator.Play(name + "_" + Directions[_direction]);
        StartCoroutine(YieldCoroutine());
    }
    private IEnumerator YieldCoroutine()
    {
        yield return new WaitForEndOfFrame();
        if (!_animator.GetCurrentAnimatorStateInfo(0).loop)
        {
            _skipAction = false;
            MovingActors.Add(this);
            StartCoroutine(AnimationCoroutine());
        } else
        {
            _animInProgress = false;
        }
    }
    private IEnumerator AnimationCoroutine()
    {
        while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 && !_skipAction)
        {
            yield return new WaitForEndOfFrame();
        }
        MovingActors.Remove(this);
        _animator.Play("idle_" + Directions[_direction]);
        _animInProgress = false;
    }

    private IEnumerator MoveCoroutine(Vector3 currPos, Vector3 newPos, float t)
    {
        _moveInProgress = true;
        PlayAnimation("walk");
        Transition transition = new Transition();
        transition.max = t;
        while (transition.Progression < 1)
        {
            transition.Progress();
            transform.position = Vector3.Lerp(currPos, newPos, transition.Progression);
            if (!_skipAction)
                yield return new WaitForEndOfFrame();
        }
        PlayAnimation("idle");
        _moveInProgress = false;
        MovingActors.Remove(this);
    }

    public void FaceActor(int direction)
    {
        _direction = direction;
        PlayAnimation("idle");
    }
    
    private void Start()
    {
        if (MovingActors == null)
            MovingActors = new List<CutsceneActor>();
        _skipAction = false;
        _moveInProgress = false;
        _animInProgress = false;
        _animator = GetComponent<Animator>();

        PlayAnimation("idle");
    }

}
