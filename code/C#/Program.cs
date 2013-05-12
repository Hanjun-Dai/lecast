using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lecast_P2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Loading queries...");
            WordProcessor.BuildWordGraph("train");
            Console.WriteLine("Building words connections...");
            ////WordProcessor.FindConnections();
            WordProcessor.BuildGlobalEquGraph();
            WordProcessor.BuildLocalEquGraph();
            Console.WriteLine("Initializing CF model...");
            CF.Initialize();
            Console.WriteLine("Initialization finishd!");

            Console.WriteLine("Begin Query");
            SkuSelector.Query();
            Console.WriteLine("All things have been done!");

            Console.ReadLine();
        }
    }
}
