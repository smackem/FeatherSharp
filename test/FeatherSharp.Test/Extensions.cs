using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.Test
{
    static class Extensions
    {
        public static T[] Singleton<T>(this T obj)
        {
            return new[] { obj };
        }
    }
}
