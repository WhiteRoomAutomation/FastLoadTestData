using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastLoadTestData
{
    class Program
    {
        static void Main(string[] args)
        {
            FastLoadSimManager myManager = new FastLoadSimManager();
            myManager.GenerateData();
        }
    }
}
