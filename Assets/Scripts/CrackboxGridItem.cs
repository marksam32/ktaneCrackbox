using System.Linq;

namespace Crackbox
{
    public class CrackboxGridItem
    {
        private CrackboxGridItem() { }
        public CrackboxGridItem(CrackboxGridItem item)
        {
            Index = item.Index;
            UpNeighbour = item.UpNeighbour;
            DownNeighbour = item.DownNeighbour;
            LeftNeighbour = item.LeftNeighbour;
            RightNeighbour = item.RightNeighbour;
            IsBlack = item.IsBlack;
            Value = item.Value;
            IsLocked = item.IsLocked;
            Neighbours = item.Neighbours;
        }

        public int Index { get; set; }
        public int UpNeighbour { get; set; }
        public int DownNeighbour { get; set; }
        public int LeftNeighbour { get; set; }
        public int RightNeighbour { get; set; }
        public bool IsBlack { get; set; }
        public int Value { get; set; }
        public bool IsLocked { get; set; }
        
        public int[] Neighbours { get; set; }

        public static CrackboxGridItem[] Clone(CrackboxGridItem[] crackboxGridItems)
        {
            return crackboxGridItems.Select(item => new CrackboxGridItem(item)).ToArray();
        }

        public static CrackboxGridItem Create(int index, int dn, int un, int ln, int rn, int[] neighbours)
        {
            return new CrackboxGridItem
            {
                Index = index,
                DownNeighbour = dn,
                UpNeighbour = un,
                LeftNeighbour = ln,
                RightNeighbour = rn,
                IsBlack = false,
                Neighbours = neighbours,
                IsLocked = false
            };
        }
    }
}
