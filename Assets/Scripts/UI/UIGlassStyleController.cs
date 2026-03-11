using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Survivebest.UI
{
    public class UIGlassStyleController : MonoBehaviour
    {
        [SerializeField] private List<Graphic> glassPanels = new();
        [SerializeField] private List<Graphic> glowAccents = new();
        [SerializeField] private Color glassTint = new(0.08f, 0.15f, 0.25f, 0.65f);
        [SerializeField] private Color glowTint = new(0.3f, 0.75f, 1f, 0.95f);

        private void OnEnable()
        {
            ApplyStyle();
        }

        public void ApplyStyle()
        {
            ApplyColor(glassPanels, glassTint);
            ApplyColor(glowAccents, glowTint);
        }

        public void SetGlassTint(Color color)
        {
            glassTint = color;
            ApplyStyle();
        }

        public void SetGlowTint(Color color)
        {
            glowTint = color;
            ApplyStyle();
        }

        private static void ApplyColor(List<Graphic> graphics, Color color)
        {
            if (graphics == null)
            {
                return;
            }

            for (int i = 0; i < graphics.Count; i++)
            {
                if (graphics[i] != null)
                {
                    graphics[i].color = color;
                }
            }
        }
    }
}
