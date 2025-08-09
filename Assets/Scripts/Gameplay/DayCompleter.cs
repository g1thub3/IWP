using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DayCompleter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    DialogueHandler _dialogueHandler;
    LevelUpHandler _levelupHandler;
    void Start()
    {
        GlobalCanvasManager.LoadInstance();

        _dialogueHandler = GlobalCanvasManager.Instance.DialogueHandler;
        _levelupHandler = GlobalCanvasManager.Instance.LevelUpHandler;
        for (int i = GlobalGameManager.Instance.inventory.Count - 1; i >= 0; i--)
        {
            if (GlobalGameManager.Instance.inventory[i].IsQuestTarget)
            {
                GlobalGameManager.Instance.inventory.RemoveAt(i);
            }
        }
        StartCoroutine(QuestCompleteSequence());
    }

    private IEnumerator QuestCompleteSequence()
    {
        if (GlobalGameManager.Instance.DayOver)
        {
            GlobalGameManager.Instance.DayOver = false;
            GlobalGameManager.Instance.CycleDay();
            DialogueData[] dayOver = new DialogueData[] { new DialogueData(new string[] { "With the day having ended, and the sun setting over the horizon, Cherry goes to sleep, hoping to wake up well-rested for another day of adventuring." }) };
            _dialogueHandler.PromptSequence(dayOver);
            while (_dialogueHandler.IsInProgress())
            {
                yield return new WaitForEndOfFrame();
            }

            bool hasQuestComplete = false;
            for (int i = GlobalGameManager.Instance.ownedQuests.Count - 1; i >= 0; i--)
            {
                var q = GlobalGameManager.Instance.ownedQuests[i];
                if (q.quest.questCompleted)
                {
                    hasQuestComplete = true;
                    string[] speech = {
                    "Cherry completed a quest from " + q.clientName + "!"
                };
                    DialogueData begin = new DialogueData(speech);
                    DialogueData[] sequence = { begin };

                    _dialogueHandler.PromptSequence(sequence);
                    while (_dialogueHandler.IsInProgress())
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    q.goldReward.Award();

                    sequence[0].content[0] = q.clientName + " awarded " + q.goldReward.Amount + " Gold!";
                    _dialogueHandler.PromptSequence(sequence);
                    while (_dialogueHandler.IsInProgress())
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    q.itemReward.Award();


                    sequence[0].content[0] = q.clientName + " awarded ";
                    for (int j = 0; j < q.itemReward.Reward.Count; j++)
                    {
                        sequence[0].content[0] += q.itemReward.Reward[j].ToString() + (j == q.itemReward.Reward.Count - 1 ? "!" : ", ");
                    }
                    _dialogueHandler.PromptSequence(sequence);
                    while (_dialogueHandler.IsInProgress())
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    q.adventurerReward.Award();
                    if (_levelupHandler.IsInProgress())
                    {
                        while (_levelupHandler.IsInProgress())
                        {
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    else
                    {
                        sequence[0].content[0] = q.clientName + " awarded " + q.adventurerReward.Amount + " Adventurer Exp!";
                        _dialogueHandler.PromptSequence(sequence);
                        while (_dialogueHandler.IsInProgress())
                        {
                            yield return new WaitForEndOfFrame();
                        }
                    }

                    GlobalGameManager.Instance.ownedQuests.RemoveAt(i);
                }
                else if (!GlobalGameManager.Instance.ownedQuests[i].quest.questPossible)
                {
                    GlobalGameManager.Instance.ownedQuests.RemoveAt(i);
                }
                yield return new WaitForEndOfFrame();
            }

            if (hasQuestComplete)
            {
                GameStoryManager.Instance.OnQuestComplete();
            }

            PromptInfo savePrompt = PromptInfo.New("Would you like to save the game?", new string[] { "Yes", "No" }, new PromptInfo.OptionFunction[]
            {
                delegate
                {
                    GlobalCanvasManager.Instance.SaveDataUIHandler.GoToMainMenu = false;
                    GlobalCanvasManager.Instance.SaveDataUIHandler.SaveMenu();
                },
                PromptInfo.NullFunction
            });
            GlobalCanvasManager.Instance.PromptHandler.Prompt(savePrompt);
            while (GlobalCanvasManager.Instance.IsInteractionActiveFreeRoam)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        GlobalCanvasManager.Instance.FadeTransition(1.0f, null, false, false);
    }
}
