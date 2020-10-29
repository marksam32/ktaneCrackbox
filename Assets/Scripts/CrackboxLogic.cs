using System;
using System.Collections.Generic;
using System.Linq;

namespace Crackbox
{
    public class CrackboxLogic
    {
        public static IList<CrackboxGridItem> CreateGrid()
        {
            return new List<CrackboxGridItem>
            {
                CrackboxGridItem.Create(0, 4, 12, 3, 1, new List<int> { 1, 4, 5 } ),
                CrackboxGridItem.Create(1, 5, 13, 0, 2, new List<int> { 0, 2, 4, 5, 6 }),
                CrackboxGridItem.Create(2, 6, 14, 1, 3, new List<int> { 1, 3, 5, 6, 7 }),
                CrackboxGridItem.Create(3, 7, 15, 2, 0, new List<int> { 2, 6, 7}),
                CrackboxGridItem.Create(4, 8, 0, 7, 5, new List<int> { 0, 1, 5, 8, 9}),
                CrackboxGridItem.Create(5, 9, 1, 4, 6, new List<int> { 0, 1, 2, 4, 6, 8, 9, 10}),
                CrackboxGridItem.Create(6, 10, 2, 5, 7, new List<int> { 1, 2, 3, 5, 7, 9, 10, 11}),
                CrackboxGridItem.Create(7, 11, 3, 6, 4, new List<int> { 2, 3, 6, 10, 11}),
                CrackboxGridItem.Create(8, 12, 4, 11, 9, new List<int> { 4, 5, 9, 12, 13}),
                CrackboxGridItem.Create(9, 13, 5, 8, 10, new List<int> { 4, 5, 6, 8, 10, 12, 13, 14}),
                CrackboxGridItem.Create(10, 14, 6, 9, 11, new List<int> { 5, 6, 7, 9, 11, 13, 14, 15}),
                CrackboxGridItem.Create(11, 15, 7, 10, 8, new List<int> { 6, 7, 10, 14, 15}),
                CrackboxGridItem.Create(12, 0, 8, 15, 13, new List<int> { 8, 9, 13}),
                CrackboxGridItem.Create(13, 1, 9, 12, 14, new List<int> { 8, 9, 10, 12, 14}),
                CrackboxGridItem.Create(14, 2, 10, 13, 15, new List<int> { 9, 10, 11, 13, 15}),
                CrackboxGridItem.Create(15, 3, 11, 14, 12, new List<int> { 10, 11, 14})
            };
        }

        public static int GetNextIndex(int currentIndex, CrackboxGridItem[] gridItems, ArrowButtonDirection direction)
        {
            int neighbour = 0;

            if (currentIndex < 0 || currentIndex + 1 > gridItems.Length)
            {
                throw new ArgumentException("Current index is incorrect " + currentIndex);
            }

            var currentGridItem = gridItems[currentIndex];

            switch (direction)
            {
                case ArrowButtonDirection.Up:
                    neighbour = currentGridItem.UpNeighbour;
                    break;
                case ArrowButtonDirection.Down:
                    neighbour = currentGridItem.DownNeighbour;
                    break;
                case ArrowButtonDirection.Right:
                    neighbour = currentGridItem.RightNeighbour;
                    break;
                case ArrowButtonDirection.Left:
                    neighbour = currentGridItem.LeftNeighbour;
                    break;
                default:
                    throw new InvalidOperationException();

            }

            //var newGridItem = gridItems[neighbour];
            //if (newGridItem.IsBlack)
            //{
            //    return GetNextIndex(neighbour, gridItems, direction);
            //}
            

            return neighbour;
        }

        public static bool IsSolved(IList<CrackboxGridItem> items)
        {
            var array = items.ToArray();

            var noValues = array.Where(x => x.Value != 0).Select(x => x.Value).ToList();
            
            if (noValues.Count() != 10)
            {
                // All values not placed.
                return false;
            }

            if (noValues.Count() != noValues.Distinct().Count())
            {
                // Duplicate values placed.
                return false;
            }

            for (int i = 0; i < array.Count(); ++i)
            {
                var item = items[i];

                if (!item.IsBlack)
                {
                    var value = item.Value;
                    var neighbours = item.Neighbours.Where(x => x > i).ToArray();
                    for (var j = 0; j < neighbours.Count(); ++j)
                    {
                        var nv = array[neighbours[j]];
                        if (nv.IsBlack)
                        {
                            // Don't care about black squares.
                            continue;
                        }

                        if (UtilityMethods.AreAdjacent(value, nv.Value))
                        {
                            // Value adjacent.
                            continue;
                        }

                        if (UtilityMethods.AreBothOddOrEven(value, nv.Value))
                        {
                            // Both are even or odd.
                            continue;
                        }

                        return false;
                    }
                }
            }

            return true;
        }
    }
}
