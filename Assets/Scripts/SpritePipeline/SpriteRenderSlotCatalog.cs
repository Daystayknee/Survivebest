namespace Survivebest.SpritePipeline
{
    public static class SpriteRenderSlotCatalog
    {
        public const int BodyBase = 100;
        public const int Neck = 150;
        public const int Head = 200;
        public const int Ears = 250;
        public const int HairBack = 300;
        public const int Eyes = 400;
        public const int Nose = 500;
        public const int Mouth = 600;
        public const int Brows = 650;
        public const int Lashes = 700;
        public const int HealthOverlay = 750;
        public const int StateOverlay = 775;
        public const int HairFront = 800;
        public const int FacialHair = 850;
        public const int Makeup = 900;
        public const int ClothingBase = 950;
        public const int Outerwear = 1000;
        public const int FxOverlay = 1100;

        public static int ToSortingOrder(SpriteRendererSlot slot)
        {
            return slot switch
            {
                SpriteRendererSlot.BodyBase => BodyBase,
                SpriteRendererSlot.Head => Head,
                SpriteRendererSlot.Eyes => Eyes,
                SpriteRendererSlot.Nose => Nose,
                SpriteRendererSlot.Mouth => Mouth,
                SpriteRendererSlot.Brows => Brows,
                SpriteRendererSlot.HairFront => HairFront,
                SpriteRendererSlot.HairSideLeft => HairFront - 20,
                SpriteRendererSlot.HairSideRight => HairFront - 20,
                SpriteRendererSlot.HairBack => HairBack,
                SpriteRendererSlot.HealthOverlay => HealthOverlay,
                SpriteRendererSlot.StateOverlay => StateOverlay,
                SpriteRendererSlot.PortraitFallback => FxOverlay,
                _ => 0
            };
        }
    }
}
