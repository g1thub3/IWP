using UnityEngine;

public class DGItemContainer : MonoBehaviour
{
    [SerializeField] private Item _data;
    public Item Item
    {
        get { return _data; }
        set { _data = value; }
    }

    private void Start()
    {
        if (_data == null)
            _data = new Item();
        _data.Set();
        if (_data.IsQuestTarget)
        {
            transform.Find("QuestTargetIcon").GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
