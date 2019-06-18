using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cocos2D
{
    public class CCRandom
    {
        // random seed 
		private static System.Random _random = new System.Random((int)DateTime.Now.Ticks);

        public static void Randomize(int seed)
        {
            _random = new System.Random(seed);
        }

        //---------------------------------------------------------------- 
        // returns int from [min to max] 
        //---------------------------------------------------------------- 
        public static int GetRandomInt(int min, int max)
        {
            return _random.Next(min, max + 1);
        }

        public static int Next()
        {
            return _random.Next();
        }

        public static int Next(int max)
        {
            return _random.Next(max);
        }

        public static int Next(int min, int max)
        {
            return _random.Next(min, max);
        }

        public static double NextDouble()
        {
            return _random.NextDouble();
        }
        
        //---------------------------------------------------------------- 
        // returns float from [min to max] 
        //---------------------------------------------------------------- 
        public static float GetRandomFloat(float min, float max)
        {
            return min + (float)_random.NextDouble() * ((max) - min);
        }

        public static float Float_0_1()
        {
            return (float)_random.Next() / int.MaxValue;
        }

        public static float Float_Minus1_1()
        {
            return (float)(_random.NextDouble() * 2.0 - 1.0);
        }
    }


}
