using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "VFXManager", menuName = "Scriptable Objects/VFX Manager")]
public class VFXManager : SingletonScriptableObject<VFXManager>
{
    [SerializeField] GameObject _animVFXPrefab;

    public GameObject Create(string name, Vector3 pos)
    {
        var newObj = Instantiate(_animVFXPrefab, pos, Quaternion.identity);
        newObj.GetComponent<SpriteAnimation>().anim = name;
        return newObj;
    }
}
