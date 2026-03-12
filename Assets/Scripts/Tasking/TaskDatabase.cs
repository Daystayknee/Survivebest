using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Tasks
{
    [CreateAssetMenu(menuName = "Survivebest/Tasks/Task Database", fileName = "TaskDatabase")]
    public class TaskDatabase : ScriptableObject
    {
        [SerializeField] private List<TaskDefinition> tasks = new();

        public IReadOnlyList<TaskDefinition> Tasks => tasks;

        public TaskDefinition FindById(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return null;
            }

            return tasks.Find(x => x != null && x.TaskId == taskId);
        }

        public List<TaskDefinition> FindByCategory(TaskCategory category)
        {
            List<TaskDefinition> results = new();
            for (int i = 0; i < tasks.Count; i++)
            {
                TaskDefinition definition = tasks[i];
                if (definition != null && definition.Category == category)
                {
                    results.Add(definition);
                }
            }

            return results;
        }
    }
}
