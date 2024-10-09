using System;

namespace CJoules
{
    class Program
    {
        static void Main(string[] args)
        {
            EnergyMeter meter = new EnergyMeter();

            meter.MeasureEnergy(TestSnippet, "TestSnippet");

            Console.WriteLine("Energy measurement completed......");
        }

        static void TestSnippet()
        {
            for (int i = 0; i < 10000000; i++) { }
        }
    }
}
