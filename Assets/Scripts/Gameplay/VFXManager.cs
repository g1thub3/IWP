using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "VFXManager", menuName = "Scriptable Objects/VFX Manager")]
public class VFXManager : SingletonScriptableObject<VFXManager>
{
    [SerializeField] GameObject _animVFXPrefab;
    [SerializeField] GameObject _animVFXLoopPrefab;

    public GameObject Create(string name, Vector3 pos, int addLayer = 0)
    {
        var newObj = Instantiate(_animVFXPrefab, pos, Quaternion.identity);
        newObj.GetComponent<SpriteAnimation>().anim = name;
        newObj.GetComponent<SpriteRenderer>().sortingOrder += addLayer;
        return newObj;
    }

    public GameObject CreateLooped(string name, Vector3 pos, int addLayer = 0)
    {
        var newObj = Instantiate(_animVFXLoopPrefab, pos, Quaternion.identity);
        newObj.GetComponent<Animator>().Play(name);
        newObj.GetComponent<SpriteRenderer>().sortingOrder += addLayer;
        return newObj;
    }
}
