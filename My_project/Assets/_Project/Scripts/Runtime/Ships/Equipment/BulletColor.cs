using System;

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct BulletColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public static BulletColor FromBytes(byte r, byte g, byte b, byte a = 255)
        {
            return new BulletColor { R = r, G = g, B = b, A = a };
        }
    }
}
