using Ionic.Zip;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTXL.Model;

namespace TMTXL.Control
{
    [Serializable]
    [ProtoContract]
    public class ResultsPackage
    {
        [ProtoMember(1)]
        public List<CSMSearchResult> CSMSearchResults;
        [ProtoMember(2)]
        public List<XLSearchResult> XLSearchResults;
        [ProtoMember(3)]
        public List<XLSearchResult> ResidueSearchResults;
        [ProtoMember(4)]
        public List<MSUltraLight> Spectra;
        [ProtoMember(5)]
        public List<string> FileNameIndex;
        [ProtoMember(6)]
        public List<ProteinProteinInteraction> PPIResults;
        

        /// <summary>
        /// Empty constructor
        /// </summary>
        public ResultsPackage()
        {
            CSMSearchResults = new List<CSMSearchResult>();
            XLSearchResults = new List<XLSearchResult>();
            ResidueSearchResults = new List<XLSearchResult>();
            Spectra = new List<MSUltraLight>();
            FileNameIndex = new List<string>();
            PPIResults = new List<ProteinProteinInteraction>();
        }

        public ResultsPackage(List<CSMSearchResult> cSMSearchResults, List<XLSearchResult> xlSearchResults, List<XLSearchResult> residueSearchResults, List<MSUltraLight> spectra, List<string> fileNameIndex, List<ProteinProteinInteraction> pPIResults)
        {
            this.CSMSearchResults = cSMSearchResults;
            this.XLSearchResults = xlSearchResults;
            this.ResidueSearchResults = residueSearchResults;
            this.Spectra = spectra;
            this.FileNameIndex = fileNameIndex;
            this.PPIResults = pPIResults;
        }

        /// <summary>
        /// Method responsible for serializing results
        /// </summary>
        /// <param name="fileName"></param>
        public void SerializeResults(string fileName)
        {
            MemoryStream fileToCompress = new MemoryStream();
            Serializer.SerializeWithLengthPrefix(fileToCompress, this, PrefixStyle.Base128, 1);

            fileToCompress.Seek(0, SeekOrigin.Begin);   // <-- must do this after writing the stream!

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (ZipFile zipFile = new ZipFile())
            {
                zipFile.Password = "7M7X4@0@!";
                zipFile.AddEntry("FileCompressed", fileToCompress);
                zipFile.Save(fileName);
            }
        }

        /// <summary>
        /// Method responsible for deserializing results
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ResultsPackage DeserializeResults(string fileName)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (ZipFile zip = ZipFile.Read(fileName))
            {
                using (var ms = new MemoryStream())
                {
                    ZipEntry entry = zip["FileCompressed"];
                    entry.ExtractWithPassword(ms, "7M7X4@0@!");// extract uncompressed content into a memorystream 

                    ms.Seek(0, SeekOrigin.Begin); // <-- must do this after writing the stream!

                    List<ResultsPackage> toDeserialize = Serializer.DeserializeItems<ResultsPackage>(ms, PrefixStyle.Base128, 1).ToList();
                    return toDeserialize[0];
                }
            }
        }
    }
}
