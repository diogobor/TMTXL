using PatternTools.MSParserLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IsobaricAnalyzer
{
    public class YadaMultiplexFinder
    {
        public Dictionary<string, List<int>> fileNameScanNumberMultiplexDictionary { get; set; }

        public YadaMultiplexFinder(DirectoryInfo yadaDir)
        {
            fileNameScanNumberMultiplexDictionary = new Dictionary<string, List<int>>();

            List<FileInfo> ms2Files = yadaDir.GetFiles("*.ms2").ToList();

            foreach (FileInfo fi in ms2Files)
            {
                var ms = PatternTools.MSParserLight.ParserUltraLightMS2.ParseSpectra(fi.FullName);
                var multiplexed = ms.FindAll(a => a.GetZLines().Count > 1);
                fileNameScanNumberMultiplexDictionary.Add(fi.Name, multiplexed.Select(a => a.ScanNumber).ToList());
                Console.WriteLine("File {0} has {1} of {2} are multiplexed spectra", fi.Name, multiplexed.Count, ms.Count);
            }
        }
    }
}
