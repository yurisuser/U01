using System.Collections.Generic;

namespace _Project.Scripts.Galaxy.Generation
{
    public sealed class ConstellationGroup
    {
        public readonly List<int> StarIndices = new List<int>();
        public int CenterStarIndex;
        public int SegmentIndex;
        public int RingIndex;
    }
}
