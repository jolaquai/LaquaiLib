namespace LaquaiLib
{
    public static class Math
    {
        public static int GCD(params int[] nums)
        {
            if (nums.Length == 1)
            {
                return nums[0];
            }
            List<int> numbers = nums.ToList().Select(n => System.Math.Abs(n)).ToList();
            if (numbers.Any(n => n == 1))
            {
                return 1;
            }

            foreach (int g in Miscellaneous.Range(numbers.Max(), 2).Cast<int>())
            {
                if (numbers.Select<int, bool>(n => n % g == 0).All())
                {
                    return g;
                }
            }
            return 1;
        }
    }
}
