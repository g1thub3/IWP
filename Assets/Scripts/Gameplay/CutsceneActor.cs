using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CutsceneActor : MonoBehaviour, IYieldable
{
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

    public bool IsInProgress()
    {
        return _moveInProgress || _animInProgress;
    }

    public void MoveActor(Transform point, float speed = 1)
    {
        float dist = (transform.position - point.position).magnitude;
        float t = dist / speed;
        StartCoroutine(MoveCoroutine(transform.position, point.position, t));
    }
    public void PlayAnimation(string name)
    {
        _animator.Play(name + "_" + Directions[_direction]);
        if (!_animator.GetCurrentAnimatorStateInfo(0).loop)
        {
            StartCoroutine(AnimationCoroutine());
        }
    }
    private IEnumerator AnimationCoroutine()
    {
        _animInProgress = true;
        var clip = _animator.GetCurrentAnimatorStateInfo(0);
        while (clip.normalizedTime < 1)
        {
            yield return new WaitForEndOfFrame();
        }
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
            yield return new WaitForEndOfFrame();
        }
        PlayAnimation("idle");
        _moveInProgress = false;
    }

    public void FaceActor(int direction)
    {
        _direction = direction;
        PlayAnimation("idle");
    }
    
    private void Start()
    {
        _moveInProgress = false;
        _animInProgress = false;
        _animator = GetComponent<Animator>();
        _direction = 0;

        PlayAnimation("idle");
    }

}
