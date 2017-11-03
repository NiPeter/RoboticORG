using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboticORG
{
    class Program
    {
        static void Main(string[] args)
        {

            Visualisation visualisation = new Visualisation();
            visualisation.Run(60, 60);
        }
    }
}
