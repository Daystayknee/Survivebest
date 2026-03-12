using System;
using System.Collections;
using UnityEngine;

namespace Survivebest.Tasks
{
    public class AutoTaskRunner : MonoBehaviour
    {
        public IEnumerator RunAuto(ActiveTaskSession session, Action<ActiveTaskSession> onComplete)
        {
            if (session == null || session.Task == null)
            {
                yield break;
            }

            yield return new WaitForSeconds(Mathf.Max(0.05f, session.Task.AutoDurationSeconds));
            session.IsCompleted = true;
            onComplete?.Invoke(session);
        }
    }
}
