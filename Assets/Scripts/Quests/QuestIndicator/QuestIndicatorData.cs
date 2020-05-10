﻿using System;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

namespace BeastHunter
{
    [CreateAssetMenu(fileName = "NewData", menuName = "CreateData/QuestIndicator", order = 0)]
    public sealed class QuestIndicatorData : ScriptableObject
    {
        #region Fields

        public const float CANVAS_OFFSET = 2.4f;

        public QuestIndicatorStruct QuestIndicatorStruct;
        public Vector3 NpcPos;
        public int NpcID;
        public DataTable DialogueCache = QuestRepository.GetDialogueCache();
        public DataTable QuestTasksCache = QuestRepository.GetQuestTaskCache();

        #endregion


        #region Methods

        public void QuestionMarkShow(bool isOn, QuestIndicatorModel model)
        {
            model.QuestIndicatorTransform.GetChild(2).gameObject.SetActive(isOn);
        }

        public void TaskQuestionMarkShow(bool isOn, QuestIndicatorModel model)
        {
            model.QuestIndicatorTransform.GetChild(1).gameObject.SetActive(isOn);
        }

        public void ExclamationMarkShow(bool isOn, QuestIndicatorModel model)
        {
            model.QuestIndicatorTransform.GetChild(0).gameObject.SetActive(isOn);
        }

        public void SetPosition(Transform npcTransform, Transform questIndicatorTransform)
        {
            questIndicatorTransform.position = new Vector3(npcTransform.position.x, npcTransform.position.y + CANVAS_OFFSET, npcTransform.position.z);
            questIndicatorTransform.parent = npcTransform;
        }

        public void QuestIndicatorCheck(GameContext context)
        {
            foreach (QuestIndicatorModel questIndicatorModel in context.QuestIndicatorModelList)
            {
                questIndicatorModel.QuestIndicatorTransform.LookAt(context.CharacterModel.CharacterCamera.transform);

                questIndicatorModel.QuestIndicatorData.GetQuestInfo(questIndicatorModel.NpcTransform.GetComponent<IGetNpcInfo>().GetInfo().Item1, questIndicatorModel);
            }
        }

        public void GetQuestInfo(int npcID, QuestIndicatorModel model)
        {

            var questModel = model.Context.QuestModel;
            var questsWithCompletedAllTask = questModel.AllTaskCompletedInQuests;
            var activeQuests = questModel.ActiveQuests;
            var completedQuests = questModel.CompletedQuests;
            var completedTasks = questModel.CompletedTasks;

            if (DialogueCache.Rows.Count != 0)
            {
                for (int i = 0; i < DialogueCache.Rows.Count; i++)
                {
                    var currentQuestID = DialogueCache.Rows[i].GetInt(8);

                    if (DialogueCache.Rows[i].GetInt(6) == 1 & DialogueCache.Rows[i].GetInt(5) == npcID)
                    {
                        if (!completedQuests.Contains(currentQuestID) & !activeQuests.Contains(currentQuestID))
                        {
                            ExclamationMarkShow(true, model);
                        }
                        else
                        {
                            ExclamationMarkShow(false, model);
                        }
                    }

                    if (DialogueCache.Rows[i].GetInt(9) == 1 & DialogueCache.Rows[i].GetInt(5) == npcID)
                    {
                        for (int j = 0; j < QuestTasksCache.Rows.Count; j++)
                        {
                            if (QuestTasksCache.Rows[j].GetInt(1) == currentQuestID)
                            {
                                var currentTaskID = QuestTasksCache.Rows[j].GetInt(0);
                                var taskTargetID = QuestTasksCache.Rows[j].GetInt(2);
                                var dialogueTargetID = DialogueCache.Rows[i].GetInt(0);
                                if (!completedTasks.Contains(currentTaskID) & activeQuests.Contains(currentQuestID) &
                                    !questsWithCompletedAllTask.Contains(currentQuestID) & taskTargetID == dialogueTargetID)
                                {
                                    TaskQuestionMarkShow(true, model);
                                }
                                else
                                {
                                    TaskQuestionMarkShow(false, model);
                                }
                            }
                        }

                    }

                    if (DialogueCache.Rows[i].GetInt(7) == 1 & DialogueCache.Rows[i].GetInt(5) == npcID)
                    {
                        if (questsWithCompletedAllTask.Contains(currentQuestID))
                        {
                            QuestionMarkShow(true, model);
                        }
                        else
                        {
                            QuestionMarkShow(false, model);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
