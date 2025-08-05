using UnityEngine;

public class InteractIndicator : MonoBehaviour
{
    [SerializeField] Transform _icon, _rotator;
    [SerializeField] float _bobSpeed = 2.0f;
    [SerializeField] float _bobAmount = 1.0f;
    [SerializeField] float _rotateSpeed = 360.0f;
    private void Update()
    {
        _icon.position = transform.position + new Vector3(0,_bobAmount * Mathf.Sin(_bobSpeed * Time.time), 0);
        _rotator.Rotate(new Vector3(0, 0, _rotateSpeed * Time.deltaTime));
    }
}
