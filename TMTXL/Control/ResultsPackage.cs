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
        private const int MAX_ITEMS_TO_BE_SAVED = 10000;

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
        [ProtoMember(7)]
        public ProgramParams Params;

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

        public ResultsPackage(List<CSMSearchResult> cSMSearchResults, List<XLSearchResult> xlSearchResults, List<XLSearchResult> residueSearchResults, List<MSUltraLight> spectra, List<string> fileNameIndex, List<ProteinProteinInteraction> pPIResults, ProgramParams _params)
        {
            this.CSMSearchResults = cSMSearchResults;
            this.XLSearchResults = xlSearchResults;
            this.ResidueSearchResults = residueSearchResults;
            this.Spectra = spectra;
            this.FileNameIndex = fileNameIndex;
            this.PPIResults = pPIResults;
            this.Params = _params;
        }

        /// <summary>
        /// Method responsible for serializing results
        /// </summary>
        /// <param name="fileName"></param>
        public void SerializeResults(string fileName)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (this.CSMSearchResults.Count > MAX_ITEMS_TO_BE_SAVED ||
                this.XLSearchResults.Count > MAX_ITEMS_TO_BE_SAVED ||
                this.ResidueSearchResults.Count > MAX_ITEMS_TO_BE_SAVED ||
                this.PPIResults.Count > MAX_ITEMS_TO_BE_SAVED ||
                this.Spectra.Count > MAX_ITEMS_TO_BE_SAVED)//big file
            {
                Console.WriteLine(" Saving results:");

                object progress_lock = new object();
                int old_progress = 0;

                using (ZipFile zipFile = new ZipFile())
                {
                    zipFile.Password = "7M7X4@0@!";
                    int fileIndex = 0;
                    int biggestLength = 0;

                    if (this.CSMSearchResults.Count > biggestLength) biggestLength = this.CSMSearchResults.Count;
                    if (this.XLSearchResults.Count > biggestLength) biggestLength = this.XLSearchResults.Count;
                    if (this.ResidueSearchResults.Count > biggestLength) biggestLength = this.ResidueSearchResults.Count;
                    if (this.PPIResults.Count > biggestLength) biggestLength = this.PPIResults.Count;
                    if (this.Spectra.Count > biggestLength) biggestLength = this.Spectra.Count;

                    //When there are many results (more than MAX_ITEMS_TO_BE_SAVED), it's necessary to split the object in different
                    //small pieces. These pieces are saved in the same zip file, but in different FileCompressed subfiles.

                    for (int count = 0; count < biggestLength; count += MAX_ITEMS_TO_BE_SAVED, fileIndex++)
                    {
                        ResultsPackage currentResults = new ResultsPackage();
                        currentResults.CSMSearchResults = this.CSMSearchResults.Skip(count).Take(MAX_ITEMS_TO_BE_SAVED).ToList();
                        currentResults.XLSearchResults = this.XLSearchResults.Skip(count).Take(MAX_ITEMS_TO_BE_SAVED).ToList();
                        currentResults.ResidueSearchResults = this.ResidueSearchResults.Skip(count).Take(MAX_ITEMS_TO_BE_SAVED).ToList();
                        currentResults.PPIResults = this.PPIResults.Skip(count).Take(MAX_ITEMS_TO_BE_SAVED).ToList();
                        currentResults.Spectra = this.Spectra.Skip(count).Take(MAX_ITEMS_TO_BE_SAVED).ToList();
                        currentResults.FileNameIndex = this.FileNameIndex;
                        currentResults.Params = this.Params;

                        MemoryStream fileToCompress = new MemoryStream();
                        Serializer.SerializeWithLengthPrefix(fileToCompress, currentResults, PrefixStyle.Base128, 1);

                        fileToCompress.Seek(0, SeekOrigin.Begin);   // <-- must do this after writing the stream!

                        string namespaceFile = "FileCompressed" + fileIndex;
                        zipFile.AddEntry(namespaceFile, fileToCompress);

                        lock (progress_lock)
                        {
                            int new_progress = (int)((double)count / (biggestLength) * 100);
                            if (new_progress > old_progress)
                            {
                                old_progress = new_progress;
                                Console.Write(" Saving results: " + old_progress + "%");
                            }
                        }
                    }

                    zipFile.AddEntry("TotalFiles", fileIndex.ToString());
                    zipFile.Save(fileName);

                    Console.WriteLine(" Saving results: 100%");
                }
            }
            else
            {
                MemoryStream fileToCompress = new MemoryStream();
                Serializer.SerializeWithLengthPrefix(fileToCompress, this, PrefixStyle.Base128, 1);

                fileToCompress.Seek(0, SeekOrigin.Begin);   // <-- must do this after writing the stream!

                using (ZipFile zipFile = new ZipFile())
                {
                    zipFile.Password = "7M7X4@0@!";
                    zipFile.AddEntry("FileCompressed", fileToCompress);
                    zipFile.Save(fileName);
                }
            }
        }

        /// <summary>
        /// Method responsible for filling the objects
        /// </summary>
        /// <param name="_rp"></param>
        private void fillObjects(ResultsPackage _rp)
        {
            this.CSMSearchResults = _rp.CSMSearchResults;
            this.FileNameIndex = _rp.FileNameIndex;
            this.Params = _rp.Params;
            this.PPIResults = _rp.PPIResults;
            this.ResidueSearchResults = _rp.ResidueSearchResults;
            this.Spectra = _rp.Spectra;
            this.XLSearchResults = _rp.XLSearchResults;
        }

        /// <summary>
        /// Method responsible for deserializing results
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public void DeserializeResults(string fileName)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            object progress_lock = new object();
            int set_processed = 0;
            int old_progress = 0;

            using (ZipFile zip = ZipFile.Read(fileName))
            {
                ZipEntry entry = zip["TotalFiles"];
                if (entry == null)//Small file
                {
                    using (var ms = new MemoryStream())
                    {
                        entry = zip["FileCompressed"];
                        entry.ExtractWithPassword(ms, "7M7X4@0@!");// extract uncompressed content into a memorystream 

                        ms.Seek(0, SeekOrigin.Begin); // <-- must do this after writing the stream!

                        List<ResultsPackage> toDeserialize = Serializer.DeserializeItems<ResultsPackage>(ms, PrefixStyle.Base128, 1).ToList();
                        this.fillObjects(toDeserialize[0]);
                    }
                }
                else
                {
                    Console.WriteLine(" Loading results:");
                    int total_files = 0;
                    using (var ms = new MemoryStream())
                    {
                        entry.ExtractWithPassword(ms, "7M7X4@0@!");// extract uncompressed content into a memorystream 
                        ms.Seek(0, SeekOrigin.Begin); // <-- must do this after writing the stream!
                        total_files = Convert.ToInt32(System.Text.ASCIIEncoding.Default.GetString(ms.GetBuffer()));
                    }
                    ResultsPackage TotalResults = new ResultsPackage();

                    for (int count = 0; count < total_files; count++)
                    {
                        using (var ms = new MemoryStream())
                        {
                            string namespace_entry = "FileCompressed" + count;
                            ZipEntry currentEntry = zip[namespace_entry];
                            currentEntry.ExtractWithPassword(ms, "7M7X4@0@!");// extract uncompressed content into a memorystream 

                            ms.Seek(0, SeekOrigin.Begin); // <-- must do this after writing the stream!

                            ResultsPackage toDeserialize = Serializer.DeserializeItems<ResultsPackage>(ms, PrefixStyle.Base128, 1).LastOrDefault();

                            if (toDeserialize != null)
                            {
                                TotalResults.FileNameIndex = toDeserialize.FileNameIndex;
                                TotalResults.Params = toDeserialize.Params;

                                if (toDeserialize.CSMSearchResults != null)
                                    TotalResults.CSMSearchResults.AddRange(toDeserialize.CSMSearchResults);
                                if (toDeserialize.XLSearchResults != null)
                                    TotalResults.XLSearchResults.AddRange(toDeserialize.XLSearchResults);
                                if (toDeserialize.ResidueSearchResults != null)
                                    TotalResults.ResidueSearchResults.AddRange(toDeserialize.ResidueSearchResults);
                                if (toDeserialize.PPIResults != null)
                                    TotalResults.PPIResults.AddRange(toDeserialize.PPIResults);
                                if (toDeserialize.Spectra != null)
                                    TotalResults.Spectra.AddRange(toDeserialize.Spectra);
                            }
                            else
                                throw new Exception("Error to load file.");
                        }

                        lock (progress_lock)
                        {
                            set_processed++;
                            int new_progress = (int)((double)set_processed / (total_files) * 100);
                            if (new_progress > old_progress)
                            {
                                old_progress = new_progress;
                                Console.Write(" Loading results: " + old_progress + "%");
                            }
                        }
                    }
                    Console.WriteLine(" Loading results: 100%");
                    this.fillObjects(TotalResults);
                }
            }
        }
    }
}
