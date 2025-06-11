using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugTools", menuName = "Scriptable Objects/DebugTools")]
public class DebugTools : SingletonScriptableObject<DebugTools>
{
    private List<GameObject> _markers;
    public bool DialogueDebugOn = false;
    public bool PromptDebugOn = false;
    public bool DungeonDebugOn = false;
    public bool EntityDebugOn = false;
    public DialogueSequence testDialogueSequence;
    public DGData placeholderDungeon;
    public GameObject debugMarker;
    public void AddMarker(Vector2 position, string text = "")
    {
        if (_markers == null) _markers = new List<GameObject>();
        var marker = Instantiate(debugMarker, position, Quaternion.identity);
        marker.name = _markers.Count.ToString();
        marker.GetComponentInChildren<TMP_Text>().text = text;
        _markers.Add(marker);
    }
    public void AddMarker(Vector2 position, Color markerCol)
    {
        if (_markers == null) _markers = new List<GameObject>();
        var marker = Instantiate(debugMarker, position, Quaternion.identity);
        marker.name = _markers.Count.ToString();
        marker.GetComponent<SpriteRenderer>().color = markerCol;
        _markers.Add(marker);
    }
    public void ClearMarkers()
    {
        if (_markers == null) return;
        foreach (var marker in _markers)
        {
            Destroy(marker);
        }
        _markers.Clear();
    }
}
