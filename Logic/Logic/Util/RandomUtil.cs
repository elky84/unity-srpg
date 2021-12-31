using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Util
{
    public class RandomUtil
    {
        private Random random = new Random();

        public int Get(int max)
        {
            return random.Next(max);
        }

        public void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
