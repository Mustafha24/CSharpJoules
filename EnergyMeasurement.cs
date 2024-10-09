using System;
using System.Collections.Generic;
using System.IO;

namespace CJoules
{
    public class EnergyMeasurement
    {
        private const string RAPL_API_DIR = "/sys/class/powercap/intel-rapl";
        private List<int> socketIdList = new List<int>();
        private List<int> availablePackageDomains = new List<int>();
        private List<int> availableDramDomains = new List<int>();

        public EnergyMeasurement()
        {
            socketIdList = GetSocketIdList();
            // socketIdList.ForEach(Console.WriteLine);
            availablePackageDomains = GetAvailablePackageDomains();
            // availablePackageDomains.ForEach(Console.WriteLine);
            availableDramDomains = GetAvailableDramDomains();
            // availableDramDomains.ForEach(Console.WriteLine);
        }

        private List<int> GetSocketIdList()
        {
            List<int> list = new List<int>();
            int socketId = 0;
            string name = $"{RAPL_API_DIR}/intel-rapl:{socketId}";

            while (Directory.Exists(name))
            {
                list.Add(socketId);
                socketId++;
                name = $"{RAPL_API_DIR}/intel-rapl:{socketId}";
            }

            if (list.Count == 0)
            {
                Console.WriteLine("Sorry! Host machine not supported..");
                Environment.Exit(0);
            }
            return list;

        }

        private List<int> GetAvailablePackageDomains()
        {
            List<int> list = new List<int>();

            foreach (var socketId in socketIdList)
            {
                string domainNameFileStr = $"{RAPL_API_DIR}/intel-rapl:{socketId}/name";
                if (File.Exists(domainNameFileStr))
                {
                    string r1 = File.ReadAllLines(domainNameFileStr)[0].Trim();
                    string packageDomain = $"package-{socketId}";

                    if (packageDomain.Equals(r1, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(socketId);
                    }
                }
            }

            return list;
        }

        private List<int> GetAvailableDramDomains()
        {
            List<int> list = new List<int>();
            string[] dramVars = { "dram", "core" };

            foreach (var socketId in socketIdList)
            {
                int domainId = 0;
                while (true)
                {
                    string domainNameFileStr = $"{RAPL_API_DIR}/intel-rapl:{socketId}/intel-rapl:{socketId}:{domainId}/name";
                    if (File.Exists(domainNameFileStr))
                    {
                        string r1 = File.ReadAllLines(domainNameFileStr)[0].Trim();
                        if (Array.Exists(dramVars, element => element.Equals(r1, StringComparison.OrdinalIgnoreCase)))
                        {
                            list.Add(socketId);
                        }
                        domainId++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return list;
        }

        public (List<long> EnergiesDram, List<long> EnergiesPackage) GetEnergyTrace()
        {
            List<long> energiesDram = new List<long>();
            List<long> energiesPackage = new List<long>();

            // DRAM Energy
            foreach (var socketId in availableDramDomains)
            {
                int domainId = 0;
                string domainNameFileStr = $"{RAPL_API_DIR}/intel-rapl:{socketId}/intel-rapl:{socketId}:{domainId}/name";
                if (File.Exists(domainNameFileStr))
                {
                    string r1 = File.ReadAllLines(domainNameFileStr)[0].Trim();
                    if (r1.Equals("dram", StringComparison.OrdinalIgnoreCase) || r1.Equals("core", StringComparison.OrdinalIgnoreCase))
                    {
                        string valuePath = $"{RAPL_API_DIR}/intel-rapl:{socketId}/intel-rapl:{socketId}:{domainId}/energy_uj";
                        if (File.Exists(valuePath))
                        {
                            if (long.TryParse(File.ReadAllLines(valuePath)[0].Trim(), out long energy))
                            {
                                energiesDram.Add(energy);
                            }
                        }
                    }
                }
            }

            // energiesDram.ForEach(Console.WriteLine);

            // Package Energy
            foreach (var socketId in availablePackageDomains)
            {
                string packageDomainValue = $"{RAPL_API_DIR}/intel-rapl:{socketId}/energy_uj";
                if (File.Exists(packageDomainValue))
                {
                    if (long.TryParse(File.ReadAllLines(packageDomainValue)[0].Trim(), out long energy))
                    {
                        energiesPackage.Add(energy);
                    }
                }
            }
            return (energiesDram, energiesPackage);
        }
    }
}
