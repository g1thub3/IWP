using UnityEngine;

public class FRNPC : MonoBehaviour
{
    [Range(0, 3)]
    [SerializeField] private int defaultDirection = 2;
    private int currDirection;

    private Animator _animator;
    private FRController _playerCharacter;

    public void FacePlayer()
    {
        if (_playerCharacter == null)
            _playerCharacter = FindAnyObjectByType<FRController>();
        currDirection = _playerCharacter.OppositeDirection;
    }
    public void ResetDirection()
    {
        currDirection = defaultDirection;
    }


    private void Start()
    {
        _animator = GetComponent<Animator>();
        ResetDirection();
    }

    private void Update()
    {
        switch(currDirection)
        {
            case 0:
                _animator.Play("idle_south");
                break;
            case 1:
                _animator.Play("idle_west");
                break;
            case 2:
                _animator.Play("idle_north");
                break;
            case 3:
                _animator.Play("idle_east");
                break;
        }
    }
}
