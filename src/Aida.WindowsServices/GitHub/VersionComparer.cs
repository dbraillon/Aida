using System;
using System.Collections.Generic;

namespace Aida.WindowsServices.GitHub
{
    public class VersionComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var vx = new Version(x);
            var vy = new Version(y);

            return vx.CompareTo(vy);
        }
    }
}
