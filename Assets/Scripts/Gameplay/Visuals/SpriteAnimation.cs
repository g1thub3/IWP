using System.Collections;
using UnityEngine;

public class SpriteAnimation : MonoBehaviour
{
    Animator _animator;
    public string anim;
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.Play(anim);
        Destroy(transform.gameObject, _animator.GetCurrentAnimatorStateInfo(0).length + 0.1f);
    }
}
