using System;
using System.Collections.Generic;

namespace ViTool.Models
{
    public class YoloObjectCounter
    {

        Dictionary<string, int> CountedObjects { get; set; } = new Dictionary<string, int>();

        public void CountObject(string defect)
        {
            if (!CountedObjects.ContainsKey(defect)) CountedObjects.Add(defect, 0);
            CountedObjects[defect]++;
        }


        public void AddCounters(YoloObjectCounter newCounter)
        {
            if (newCounter == null) return;

            foreach (KeyValuePair<string, int> ObjectAndCountPair in newCounter.CountedObjects)
            {
                if (!CountedObjects.ContainsKey(ObjectAndCountPair.Key)) CountedObjects.Add(ObjectAndCountPair.Key, 0);
                CountedObjects[ObjectAndCountPair.Key] += ObjectAndCountPair.Value;
            }

        }

        public string GetCounter() 
        {
            string counter = "";
            foreach (KeyValuePair<string, int> ObjectAndCountPair in CountedObjects)
                counter += $"Object: {ObjectAndCountPair.Key} count: {ObjectAndCountPair.Value}\n";
            return counter;
        }

        public void PrintCounters()
        {
            Console.WriteLine(" ");
            Console.WriteLine("Stats:");

            foreach (KeyValuePair<string, int> ObjectAndCountPair in CountedObjects)
                Console.WriteLine($" Object: {ObjectAndCountPair.Key} count: {ObjectAndCountPair.Value}");

            Console.WriteLine(" ");

        }

        public void PrintCountersWithPercents()
        {
            int Sum = 0;
            foreach (KeyValuePair<string, int> ObjectAndCountPair in CountedObjects) Sum += ObjectAndCountPair.Value;

            Console.WriteLine(" ");
            Console.WriteLine("Stats:");
            Console.WriteLine("All objects: " + Sum.ToString());

            foreach (KeyValuePair<string, int> ObjectAndCountPair in CountedObjects)
                Console.WriteLine($" Object:{ObjectAndCountPair.Key} count:{(ObjectAndCountPair.Value / (double)Sum).ToString("0.##%")} => {ObjectAndCountPair.Value} / {Sum}");

            Console.WriteLine(" ");

        }
    }
}


