using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine
{
	public static class Debug
	{
        public static void Log(object o)
        {
            Console.WriteLine(o.ToString());
        }

        public static void LogWarning(object o)
        {
            Console.WriteLine("WARNING: " + o.ToString());
        }

        public static void LogError(object o)
        {
            Console.WriteLine("ERROR: " + o.ToString());
        }
	}
}
