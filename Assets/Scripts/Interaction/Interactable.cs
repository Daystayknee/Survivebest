using UnityEngine;

namespace Survivebest.Interaction
{
    public enum InteractableType
    {
        Character,
        Toilet,
        Fridge,
        Bed,
        Sink
    }

    public class Interactable : MonoBehaviour
    {
        [SerializeField] private InteractableType interactableType;

        public InteractableType Type => interactableType;
    }
}
