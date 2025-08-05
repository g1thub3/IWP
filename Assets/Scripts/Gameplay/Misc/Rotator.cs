using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] float _speed = 50.0f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0,0,_speed * Time.deltaTime));
    }
}
