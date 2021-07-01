/**
 * Program:     Retrieve protein domains from Uniprot server
 * Created by:  Diogo Borges Lima
 * Description: Class responsible for parsing the JSON object retrieved from PFam server
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Uniprot
{
    public class XMLParser
    {
        public bool isPFamServer { get; set; } = true;
        public string ProteinSequence { get; set; }
        public short ProteinSeqLength { get; set; }
        //<domain_name, startIndex, endIndex, e-value>
        public List<Tuple<string, short, short, double>> Domains { get; set; }

        public string ResultProteinFromPfam { get; set; }

        public void ReadXML_Accession(string xml)
        {
            ReadXML_Accession_FromUniprot(xml);
        }

        private void ReadXML_Accession_FromUniprot(string xml)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnodes;
            xmldoc.LoadXml(xml);

            #region Check if exists error
            xmlnodes = xmldoc.GetElementsByTagName("error");
            if (xmlnodes.Count > 0)
                throw new Exception(xmlnodes[0].InnerText);
            #endregion

            #region Protein sequence
            xmlnodes = xmldoc.GetElementsByTagName("sequence");

            if (xmlnodes.Count == 0)//There is no result.
                throw new Exception("WARN: There is no protein sequence.");

            try
            {
                ProteinSeqLength = (short)Convert.ToInt32(xmlnodes[xmlnodes.Count - 1].Attributes[0].InnerText);
                ProteinSequence = xmlnodes[xmlnodes.Count - 1].InnerText;
            }
            catch (Exception) { }
            #endregion
        }

        private void ReadXML_Accession_FromPFam(string xml)
        {
            Domains = new List<Tuple<string, short, short, double>>();
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnodes;
            xmldoc.LoadXml(xml);

            #region Check if exists error
            xmlnodes = xmldoc.GetElementsByTagName("error");
            if (xmlnodes.Count > 0)
                throw new Exception(xmlnodes[0].InnerText);
            #endregion

            #region Protein sequence
            xmlnodes = xmldoc.GetElementsByTagName("sequence");

            if (xmlnodes.Count == 0)//There is no result.
                throw new Exception("WARN: There is no protein sequence.");

            ProteinSeqLength = (short)Convert.ToInt32(xmlnodes[0].Attributes[0].InnerText);
            ProteinSequence = xmlnodes[0].InnerText;
            #endregion

            #region Domains
            xmlnodes = xmldoc.GetElementsByTagName("matches");
            for (int i = 0; i < xmlnodes.Count; i++)//<matches>
            {
                for (int j = 0; j < xmlnodes[i].ChildNodes.Count; j++)//<match>
                {
                    string domain = xmlnodes[i].ChildNodes[j].Attributes[1].InnerText;
                    short startId = (short)Convert.ToInt32(xmlnodes[i].ChildNodes[j].ChildNodes[0].Attributes[0].InnerText);
                    short endId = (short)Convert.ToInt32(xmlnodes[i].ChildNodes[j].ChildNodes[0].Attributes[1].InnerText);
                    double eValue = Convert.ToDouble(xmlnodes[i].ChildNodes[j].ChildNodes[0].Attributes[6].InnerText);
                    Domains.Add(Tuple.Create(domain, startId, endId, eValue));
                }
            }
            #endregion
        }

        private void ReadXML_Accession_FromSupFam(string xml)
        {
            Domains = new List<Tuple<string, short, short, double>>();
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnodes;
            xmldoc.LoadXml(xml);

            #region Check if exists error
            xmlnodes = xmldoc.GetElementsByTagName("UNKNOWNSEGMENT");
            if (xmlnodes.Count > 0)
                throw new Exception(xmlnodes[0].InnerText);
            #endregion

            #region Protein sequence

            ProteinSeqLength = 0;
            ProteinSequence = "";
            #endregion

            #region Domains
            xmlnodes = xmldoc.GetElementsByTagName("FEATURE");
            for (int i = 0; i < xmlnodes.Count; i++)//<FEATURE>
            {
                if (!xmlnodes[i].ChildNodes[1].Attributes[0].InnerText.EndsWith("SUPERFAMILY_")) continue;
                //<TYPE>,<METHOD>,<START>,<END>,<SCORE>
                string domain = xmlnodes[i].ChildNodes[0].Attributes[0].InnerText;
                short startId = (short)Convert.ToInt32(xmlnodes[i].ChildNodes[2].InnerText);
                short endId = (short)Convert.ToInt32(xmlnodes[i].ChildNodes[3].InnerText);
                double eValue = Convert.ToDouble(xmlnodes[i].ChildNodes[4].InnerText);
                Domains.Add(Tuple.Create(domain, startId, endId, eValue));
            }
            #endregion
        }

        public void ReadXML_Sequence(string xml)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnodes;
            xmldoc.LoadXml(xml);

            xmlnodes = xmldoc.GetElementsByTagName("result_url");
            string url = xmlnodes[0].InnerText;

            if (!url.Contains("pfam.xfam.org"))
                url = "https://pfam.xfam.org" + url;

            ResultProteinFromPfam = url;

        }

        public void ReadXML_ResponseFromUniprot(string xml)
        {
            Domains = new List<Tuple<string, short, short, double>>();
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnodes;
            xmldoc.LoadXml(xml);

            #region Check if exists error
            xmlnodes = xmldoc.GetElementsByTagName("error");
            if (xmlnodes.Count > 0)
                throw new Exception(xmlnodes[0].InnerText);
            #endregion

            #region Protein sequence
            xmlnodes = xmldoc.GetElementsByTagName("protein");

            if (xmlnodes.Count == 0)//There is no result.
                return;

            ProteinSeqLength = (short)Convert.ToInt32(xmlnodes[0].Attributes[0].InnerText);
            #endregion

            #region Domains
            xmlnodes = xmldoc.GetElementsByTagName("match");
            for (int i = 0; i < xmlnodes.Count; i++)//<match>
            {
                string domain = xmlnodes[i].Attributes[1].InnerText;
                short startId = (short)Convert.ToInt32(xmlnodes[i].ChildNodes[0].Attributes[0].InnerText);
                short endId = (short)Convert.ToInt32(xmlnodes[i].ChildNodes[0].Attributes[1].InnerText);
                double eValue = Convert.ToDouble(xmlnodes[i].ChildNodes[0].Attributes[6].InnerText);
                Domains.Add(Tuple.Create(domain, startId, endId, eValue));
            }
            #endregion
        }
    }
}