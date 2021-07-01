/**
 * Program:     Retrieve protein domains from Uniprot server
 * Created by:  Diogo Borges Lima
 * Description: Class responsible for connecting to Uniprot server
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uniprot.Model;

namespace Uniprot
{
    public class Connection
    {
        /// <summary>
        /// Public variable
        /// </summary>
        public List<Protein> Proteins { get; set; }

        /// <summary>
        /// Private variable
        /// </summary>
        private StringBuilder errorMsg { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Connection()
        {
            this.errorMsg = new StringBuilder();
        }

        /// <summary>
        /// Method responsible for connecting the server(s)
        /// </summary>
        public void Connect()
        {
            if (Proteins == null || Proteins.Count == 0)
            {
                throw new Exception("WARN: There is no proteins to be retrived.");
            }
            foreach (Protein ptn in Proteins)
            {
                try
                {
                    if (!String.IsNullOrEmpty(ptn.AccessionNumber))
                        Connect_AccessionNumber(ptn);
                    else
                        Connect_ProteinSequence(ptn);
                }
                catch (Exception e)
                {
                    errorMsg.AppendLine(e.Message);
                }
            }

            if (!String.IsNullOrEmpty(errorMsg.ToString()))
                throw new Exception(errorMsg.ToString());
        }

        /// <summary>
        /// Method responsible for retrieve data using protein accession number
        /// </summary>
        /// <param name="ptn"></param>
        /// <returns></returns>
        private Task Connect_AccessionNumber(Protein ptn)
        {
            using (var client = new HttpClient())
            {
                XMLParser xmlParser = new XMLParser();
                Task<HttpResponseMessage> result = client.PostAsync("https://www.uniprot.org/uniprot/" + ptn.AccessionNumber + ".xml", new StringContent(""));
                var responseJson = result.Result.Content.ReadAsStringAsync().Result;

                try
                {
                    xmlParser.ReadXML_Accession(responseJson);
                }
                catch (Exception) { }

                ptn.ProteinLength = xmlParser.ProteinSeqLength;
                ptn.Sequence = xmlParser.ProteinSequence;

            }
            return null;
        }

        /// <summary>
        /// Method responsible for retrieve data using protein sequence
        /// </summary>
        /// <param name="ptn"></param>
        /// <returns></returns>
        private static Task Connect_ProteinSequence(Protein ptn)
        {
            XMLParser xmlParser = new XMLParser();

            using (var client = new HttpClient())
            {
                var result = client.PostAsync("https://pfam.xfam.org/search/sequence?seq=" + ptn.Sequence + "&output=xml", new StringContent(""));
                var responseJson = result.Result.Content.ReadAsStringAsync().Result;
                xmlParser.ReadXML_Sequence(responseJson);

                result = client.PostAsync(xmlParser.ResultProteinFromPfam, new StringContent(""));
                responseJson = result.Result.Content.ReadAsStringAsync().Result;
                int count = 0;
                while (!responseJson.Contains("xml") && count < 5)
                {
                    Thread.Sleep(5000);
                    result = client.PostAsync(xmlParser.ResultProteinFromPfam, new StringContent(""));
                    responseJson = result.Result.Content.ReadAsStringAsync().Result;
                    count++;
                }

                if (responseJson.Contains("xml"))
                {
                    xmlParser.ReadXML_ResponseFromUniprot(responseJson);
                    ptn.ProteinLength = xmlParser.ProteinSeqLength;
                }
                else
                    throw new Exception("No domain has been found!");
            }

            return null;
        }
    }
}