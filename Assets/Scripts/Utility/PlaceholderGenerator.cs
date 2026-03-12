using UnityEngine;
using UnityEngine.UI;

namespace Survivebest.Utility
{
    public class PlaceholderGenerator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] spriteRenderers;
        [SerializeField] private Image[] uiImages;
        [SerializeField] private Color fallbackColor = new(0.5f, 0.8f, 0.9f, 1f);
        [SerializeField] private bool randomizeColorPerSlot = true;

        private Sprite generatedSprite;

        private void Awake()
        {
            EnsureSprite();
            AssignPlaceholders();
        }

        [ContextMenu("Assign Placeholders")]
        public void AssignPlaceholders()
        {
            EnsureSprite();

            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                SpriteRenderer renderer = spriteRenderers[i];
                if (renderer == null || renderer.sprite != null)
                {
                    continue;
                }

                renderer.sprite = generatedSprite;
                renderer.color = PickColor(i);
            }

            if (uiImages == null || uiImages.Length == 0)
            {
                uiImages = GetComponentsInChildren<Image>(true);
            }

            for (int i = 0; i < uiImages.Length; i++)
            {
                Image image = uiImages[i];
                if (image == null || image.sprite != null)
                {
                    continue;
                }

                image.sprite = generatedSprite;
                image.color = PickColor(i + 100);
            }
        }

        private void EnsureSprite()
        {
            if (generatedSprite != null)
            {
                return;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            generatedSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            generatedSprite.name = "GeneratedPlaceholderSquare";
        }

        private Color PickColor(int seed)
        {
            if (!randomizeColorPerSlot)
            {
                return fallbackColor;
            }

            Random.State cached = Random.state;
            Random.InitState(seed * 7919 + 17);
            Color color = Random.ColorHSV(0f, 1f, 0.3f, 0.9f, 0.7f, 1f);
            Random.state = cached;
            return color;
        }
    }
}
