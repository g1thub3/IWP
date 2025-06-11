using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class DialogueData
{
    public DialogueData()
    {
        character = CHARACTER_ENUM.NUM_CHARACTERS;
    }

    public void ImplementCharacter()
    {
        if (character == CHARACTER_ENUM.NUM_CHARACTERS) return;
        CharacterProfile profile = CharacterProfiles.Instance.characterProfiles[(int)character];
        speakerSprite = profile.characterSprite;
        speakerName = profile.characterName;
    }

    [Header("Speaker")]
    public CHARACTER_ENUM character;
    public Sprite speakerSprite;
    public string speakerName;

    [Header("Content")]
    public string[] content;
    public float waitTime;
    public float textSpeed;

    [Header("Settings")]
    public bool isRight;
    public bool isSpriteShowing;
    public bool canSkip;
    public bool autoNext;
}

[CreateAssetMenu(fileName = "DialogueSequence", menuName = "Scriptable Objects/DialogueSequence")]
public class DialogueSequence : ScriptableObject
{
    public DialogueData[] sequence;
}