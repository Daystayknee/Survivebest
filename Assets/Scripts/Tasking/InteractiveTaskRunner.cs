using System;
using System.Collections;
using UnityEngine;

namespace Survivebest.Tasks
{
    public class InteractiveTaskRunner : MonoBehaviour
    {
        public event Action<ActiveTaskSession, TaskStepDefinition> OnStepPrompted;
        public event Action<ActiveTaskSession, TaskStepDefinition> OnStepCompleted;

        public IEnumerator RunInteractive(ActiveTaskSession session, Action<ActiveTaskSession> onComplete)
        {
            if (session == null || session.Task == null)
            {
                yield break;
            }

            for (int i = 0; i < session.Task.InteractiveSteps.Count; i++)
            {
                session.CurrentStepIndex = i;
                TaskStepDefinition step = session.Task.InteractiveSteps[i];
                OnStepPrompted?.Invoke(session, step);

                yield return new WaitForSeconds(Mathf.Max(0.05f, step.ExpectedDurationSeconds));

                if (!string.IsNullOrWhiteSpace(step.StepId))
                {
                    session.CompletedStepIds.Add(step.StepId);
                }

                OnStepCompleted?.Invoke(session, step);
            }

            session.IsCompleted = true;
            onComplete?.Invoke(session);
        }
    }
}
