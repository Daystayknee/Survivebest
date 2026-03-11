using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Society;

namespace Survivebest.Location
{
    [System.Serializable]
    public class Room
    {
        public string RoomName;
        public string AreaName = "Default";
        public Sprite Background;
        public Transform SpawnPoint;
    }

    public class LocationManager : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private List<Room> rooms = new();
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private Image fadeOverlay;
        [SerializeField, Min(0f)] private float fadeDuration = 0.25f;

        private Coroutine transitionRoutine;

        public void NavigateToRoom(string roomName)
        {
            Room room = rooms.Find(r => r.RoomName == roomName);
            if (room == null)
            {
                Debug.LogWarning($"Room not found: {roomName}");
                return;
            }

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            transitionRoutine = StartCoroutine(TransitionToRoom(room));
        }

        private IEnumerator TransitionToRoom(Room room)
        {
            yield return Fade(0f, 1f);

            if (backgroundRenderer != null && room.Background != null)
            {
                backgroundRenderer.sprite = room.Background;
            }

            if (lawSystem != null)
            {
                lawSystem.SetCurrentArea(room.AreaName);
            }

            if (householdManager != null && room.SpawnPoint != null)
            {
                foreach (CharacterCore member in householdManager.Members)
                {
                    if (member != null)
                    {
                        member.transform.position = room.SpawnPoint.position;
                    }
                }
            }

            yield return Fade(1f, 0f);
            transitionRoutine = null;
        }

        private IEnumerator Fade(float start, float end)
        {
            if (fadeOverlay == null)
            {
                yield break;
            }

            float elapsed = 0f;
            Color c = fadeOverlay.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
                c.a = Mathf.Lerp(start, end, t);
                fadeOverlay.color = c;
                yield return null;
            }

            c.a = end;
            fadeOverlay.color = c;
        }
    }
}
