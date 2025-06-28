using System.Collections;
using UnityEngine;

public class DayCompleter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    DialogueHandler _dialogueHandler;
    LevelUpHandler _levelupHandler;
    void Start()
    {
        GlobalCanvasManager.LoadInstance();

        if (GlobalGameManager.Instance.DayOver)
        {
            _dialogueHandler = GlobalCanvasManager.Instance.DialogueHandler;
            _levelupHandler = GlobalCanvasManager.Instance.LevelUpHandler;

            GlobalGameManager.Instance.DayOver = false;

            for (int i = GlobalGameManager.Instance.inventory.Count - 1; i >= 0; i--)
            {
                if (GlobalGameManager.Instance.inventory[i].IsQuestTarget)
                {
                    GlobalGameManager.Instance.inventory.RemoveAt(i);
                }
            }

            StartCoroutine(QuestCompleteSequence());
        }
        GlobalGameManager.Instance.CycleDay();
    }

    private IEnumerator QuestCompleteSequence()
    {
        for (int i = GlobalGameManager.Instance.ownedQuests.Count - 1; i >= 0; i--)
        {
            var q = GlobalGameManager.Instance.ownedQuests[i];
            if (q.quest.questCompleted)
            {
                string[] speech = {
                    "Cherry completed a quest from " + q.clientName + "!"
                };
                DialogueData begin = new DialogueData(speech);
                DialogueData[] sequence = { begin };

                _dialogueHandler.PromptSequence(sequence);
                while (_dialogueHandler.IsSequenceRunning)
                {
                    yield return new WaitForEndOfFrame();
                }

                q.goldReward.Award();

                sequence[0].content[0] = q.clientName + " awarded " + q.goldReward.Amount + " Gold!";
                _dialogueHandler.PromptSequence(sequence);
                while (_dialogueHandler.IsSequenceRunning)
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
                while (_dialogueHandler.IsSequenceRunning)
                {
                    yield return new WaitForEndOfFrame();
                }

                q.adventurerReward.Award();
                if (_levelupHandler.SequenceInProgress)
                {
                    while (_levelupHandler.SequenceInProgress)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                } else
                {
                    sequence[0].content[0] = q.clientName + " awarded " + q.adventurerReward.Amount + " Adventurer Exp!";
                    _dialogueHandler.PromptSequence(sequence);
                    while (_dialogueHandler.IsSequenceRunning)
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
    }
}
