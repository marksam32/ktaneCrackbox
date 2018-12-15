using System;
using System.Linq;

namespace Crackbox
{
    public static class UtilityMethods
    {
        public static int[] FindCandidates(int number, int[] available)
        {
            return available.Where(x => AreAdjacent(x, number) || AreBothOddOrEven(x, number)).ToArray();
        }

        public static bool IsEven(int value)
        {
            return value % 2 == 0;
        }

        public static bool IsOdd(int value)
        {
            return !IsEven(value);
        }

        public static bool AreBothOddOrEven(int value1, int value2)
        {
            return IsEven(value1) && IsEven(value2) || IsOdd(value1) && IsOdd(value2);
        }

        public static bool AreAdjacent(int value1, int value2)
        {
            return (value1 == 10 && value2 == 1) || (value1 == 1 && value2 == 10) || (Math.Abs(value1 - value2) == 1);
        }
    }
}
