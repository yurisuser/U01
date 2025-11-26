using System;

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct BulletSpec
    {
        public BulletType Type;
        public BulletColor Color;
        public RenderStyle Style;
    }
}
