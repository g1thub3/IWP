#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameStoryManager))]
public class GameStoryManagerEditor : Editor
{
    private int selectedStory = 0;
    private int selectedState = 0;
    GameStoryManager myTarget;
    //private void DrawDropdown(Rect position, GUIContent label)
    //{
    //    if (!EditorGUI.DropdownButton(position, label, FocusType.Passive))
    //    {
    //        return;
    //    }

    //    void handleItemClicked(object parameter)
    //    {
    //        Debug.Log(paramater);
    //    }

    //    GenericMenu menu = new GenericMenu();
    //    menu.AddItem(new GUIContent("Item 1"), false, handleItemClicked, "Item 1");
    //    menu.AddItem(new GUIContent("Item 2"), false, handleItemClicked, "Item 2");
    //    menu.AddItem(new GUIContent("Item 3"), false, handleItemClicked, "Item 3");
    //    menu.DropDown(position);
    //}



    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        myTarget = target as GameStoryManager;
        if (myTarget.ActiveStories.Count < 1)
            return;
        Rect vert = EditorGUILayout.BeginVertical();
        GenericMenu stories = new GenericMenu();
        for (int i = 0; i < myTarget.ActiveStories.Count; i++)
        {
            int value = i;
            stories.AddItem(new GUIContent(myTarget.ActiveStories[i].storyName), false, delegate
            {
                selectedStory = value;
            });
        }
        if (EditorGUILayout.DropdownButton(new GUIContent(myTarget.ActiveStories[selectedStory].storyName), FocusType.Passive))
        {
            stories.DropDown(vert);
        }
        EditorGUILayout.EndVertical();

        Rect vert2 = EditorGUILayout.BeginVertical();
        GenericMenu states = new GenericMenu();
        var foundStates = myTarget.ActiveStories[selectedStory].foundStoryline.storyStates;
        for (int i = 0; i < foundStates.Length; i++)
        {
            int value = i;
            states.AddItem(new GUIContent(foundStates[i]), false, delegate
            {
                selectedState = value;
            });
        }
        if (EditorGUILayout.DropdownButton(new GUIContent(foundStates[selectedState]), FocusType.Passive))
        {
            states.DropDown(vert2);
        }
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Set State"))
        {
            myTarget.ActiveStories[selectedStory].currentState = selectedState;
        }
    }
}
#endif