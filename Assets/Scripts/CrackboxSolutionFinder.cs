using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Crackbox
{
    public class CrackboxSolutionFinder
    {
        private IList<int> remaining;

        public CrackboxGridItem[] FindSolution(CrackboxGridItem[] items)
        {
            items = CrackboxLogic.CreateGrid().ToArray();

            var index = GetNextFreeIndex(items);
            this.remaining = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var value = GetStartingValue(remaining);

            items[index].Value = value;

            while (remaining.Any() && index != -1)
            {
                index = this.PlaceItem(items, index);
            }

            if (!remaining.Any())
            {
                items.ForEach(item =>
                {
                    if (item.Value == 0)
                    {
                        item.IsBlack = true;
                    }
                });

                return items;
            }

            return null;
        }

        public static CrackboxGridItem[] Anonymize(CrackboxGridItem[] items)
        {
            var number1 = GetNextFreeIndex(items);
            var number2 = GetNextFreeIndex(items);
            while (number1 == number2)
            {
                number2 = GetNextFreeIndex(items);
            }

            items.ForEach(item =>
            {
                if (!item.IsBlack && item.Index != number1 && item.Index != number2)
                {
                    item.Value = 0;
                }

                if (!item.IsBlack && (item.Index == number1 || item.Index == number2))
                {
                    item.IsLocked = true;
                }
            });

            return items;
        }

        private int PlaceItem(CrackboxGridItem[] items, int index)
        {
            CrackboxGridItem next = null;
            var neighbours = Shuffle(items[index].Neighbours).ToArray();
            foreach (var neighbour in neighbours)
            {
                if (items[neighbour].Value == 0)
                {
                    next = items[neighbour];
                }
                else
                {
                    // That place was already taken.
                    continue;
                }

                var candidates = Shuffle(UtilityMethods.FindCandidates(items[index].Value, this.remaining.ToArray())).ToArray();

                foreach (var candidate in candidates)
                {
                    if (next.Value != 0)
                    {
                        throw new InvalidOperationException(string.Format("Trying to place value in index {0} that already has value.", next.Index));
                    }

                    var nextNeighbours = Shuffle(next.Neighbours);
                    if (CanPlace(candidate, nextNeighbours, items))
                    {
                        this.remaining.Remove(candidate);
                        next.Value = candidate;
                        index = next.Index;

                        return index;
                    }
                }
            }

            return -1;
        }

        internal static bool CanPlace(int value, IList<int> neighbours, CrackboxGridItem[] items)
        {
            // Find those neighbours that has a value.
            var neighboursWithValue = neighbours.Select(neighbour => items[neighbour]).Where(item => item.Value != 0).ToList();
            foreach (var neighbourWithValue in neighboursWithValue)
            {
                if (!UtilityMethods.AreAdjacent(value, neighbourWithValue.Value) && !UtilityMethods.AreBothOddOrEven(value, neighbourWithValue.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetNextFreeIndex(CrackboxGridItem[] items)
        {
            var number = UnityEngine.Random.Range(0, 16);
            while (items[number].IsBlack)
            {
                number = UnityEngine.Random.Range(0, 16);
            }

            return number;
        }

        private static int GetStartingValue(IList<int> remaining)
        {
            var value = UnityEngine.Random.Range(1, 11);
            remaining.Remove(value);
            return value;
        }

        private static IList<T> Shuffle<T>(IList<T> list)
        {
            int count = list.Count;
            while (count > 1)
            {
                count--;
                var next = UnityEngine.Random.Range(0, count + 1);
                T value = list[next];
                list[next] = list[count];
                list[count] = value;
            }

            return list;
        }
    }
}

