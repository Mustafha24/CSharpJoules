using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CJoules
{
    public class EnergyMeter
    {
        private EnergyMeasurement energyMeasurement;
        private string outputFile = "CJoules_output.csv";

        public EnergyMeter()
        {
            energyMeasurement = new EnergyMeasurement();
            InitializeOutputFile();
        }

        private void InitializeOutputFile()
        {
            if (!File.Exists(outputFile))
            {
                string header = "new_tag,timestamp,time,package0,package1,dram0,dram1\n";
                File.WriteAllText(outputFile, header);
            }
        }

        public void MeasureEnergy(Action codeSnippet, string tag)
        {
            var traceStart = energyMeasurement.GetEnergyTrace();
            DateTime startTime = DateTime.Now;
            Console.WriteLine($"Time_stamp: {startTime.ToString("o", CultureInfo.InvariantCulture)}");

            codeSnippet.Invoke();

            DateTime endTime = DateTime.Now;
            var traceEnd = energyMeasurement.GetEnergyTrace();

            TimeSpan timeElapsed = endTime - startTime;
            Console.WriteLine($"Time Elapsed: {timeElapsed}");

            List<long> dramDiff = CalculateDifferences(traceStart.EnergiesDram, traceEnd.EnergiesDram);
            List<long> packageDiff = CalculateDifferences(traceStart.EnergiesPackage, traceEnd.EnergiesPackage);

            string newTag = tag;
            DateTime timestamp = startTime;


            double time = timeElapsed.TotalSeconds;

            string package0 = packageDiff.Count > 0 ? packageDiff[0].ToString() : "0";
            string package1 = packageDiff.Count > 1 ? packageDiff[1].ToString() : "0";
            string dram0 = dramDiff.Count > 0 ? dramDiff[0].ToString() : "0";
            string dram1 = dramDiff.Count > 1 ? dramDiff[1].ToString() : "0";

            string csvLine = $"{newTag},{timestamp},{time},{package0},{package1},{dram0},{dram1}\n";
            File.AppendAllText(outputFile, csvLine);
        }

        private List<long> CalculateDifferences(List<long> startValues, List<long> endValues)
        {
            List<long> differences = new List<long>();
            int count = Math.Min(startValues.Count, endValues.Count);

            for (int i = 0; i < count; i++)
            {
                differences.Add(endValues[i] - startValues[i]);
            }

            for (int i = count; i < endValues.Count; i++)
            {
                differences.Add(endValues[i]);
            }

            return differences;
        }
    }
}
