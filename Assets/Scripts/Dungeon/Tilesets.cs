using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Tileset
{
    public GameObject tileSprite;
    public Color floorColour;
    public Color wallColour;
}

[CreateAssetMenu(fileName = "Tilesets", menuName = "Scriptable Objects/Tilesets")]
public class Tilesets : SingletonScriptableObject<Tilesets>
{
    public enum TILESET
    {
        BEGONIA_FOREST,
        COBWEB_CAVE,
        NUM_SETS
    }

    public KeyDataList structureList;
    public KeyDataList itemList;

    [SerializeField] private GameObject _structureTemplate;
    [SerializeField] private GameObject _itemTemplate;
    public GameObject ConstructItemInteractable(string itemKey)
    {
        var newInteractable = Instantiate(_itemTemplate);
        var newData = new Item();
        newData.itemKey = itemKey;
        newData.Implement();
        newInteractable.GetComponent<SpriteRenderer>().sprite = newData.module.itemSprite;
        newInteractable.GetComponent<DGItemContainer>().Item = newData;
        return newInteractable;
    }
    public GameObject ConstructItemInteractable(Item data)
    {
        var newInteractable = Instantiate(_itemTemplate);
        data.Set();
        newInteractable.GetComponent<SpriteRenderer>().sprite = data.module.itemSprite;
        newInteractable.GetComponent<DGItemContainer>().Item = data;
        return newInteractable;
    }
    public GameObject ConstructStructure(DGInteraction interaction)
    {
        var newInteractable = Instantiate(_structureTemplate);
        newInteractable.GetComponent<DGInteractable>().interaction = interaction;
        return newInteractable;
    }

    public Tileset[] tilesets = new Tileset[(int)TILESET.NUM_SETS];
}
