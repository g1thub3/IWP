using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class DialogueData
{
    [HideInInspector] public string name;
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

    public DialogueData(string[] speech, 
        CHARACTER_ENUM character = CHARACTER_ENUM.NUM_CHARACTERS,
        Sprite speakerSprite = null,
        string speakerName = "",
        float waitTime = 2,
        float textSpeed = 0.05f,
        bool isRight = true,
        bool isSpriteShowing = false,
        bool canSkip = true,
        bool autoNext = false)
    {
        content = speech;

        this.character = character;
        this.speakerSprite = speakerSprite;
        this.speakerName = speakerName;
        ImplementCharacter();

        this.waitTime = waitTime;
        this.textSpeed = textSpeed;
        this.isRight = isRight;
        this.isSpriteShowing = isSpriteShowing;
        this.canSkip = canSkip;
        this.autoNext = autoNext;
    }
}

[CreateAssetMenu(fileName = "DialogueSequence", menuName = "Scriptable Objects/DialogueSequence")]
public class DialogueSequence : ScriptableObject
{
    public DialogueData[] sequence;
    public void Prompt()
    {
        GlobalCanvasManager.Instance.DialogueHandler.PromptSequence(sequence);
    }
}