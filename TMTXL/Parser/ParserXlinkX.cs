using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMTXL.Model;

namespace TMTXL.Parser
{
    /// <summary>
    /// Model class for XlinkX results
    /// </summary>
    public static class ParserXlinkX
    {
        #region CSM
        private static CSMSearchResult current_csm;
        private static string[] HeaderLineCSM { get; set; }
        private static string _index { get; set; }
        private static short fileIndex { get; set; }
        private static int scanNumber { get; set; }
        private static int charge { get; set; }
        private static double precursor_mass { get; set; }
        private static string peptide_alpha { get; set; }
        private static string peptide_beta { get; set; }
        private static short pep_alpha_pos { get; set; }
        private static short ptn_alpha_pos { get; set; }
        private static short pep_beta_pos { get; set; }
        private static short ptn_beta_pos { get; set; }
        private static string protein_alpha { get; set; }
        private static string protein_beta { get; set; }
        private static double peptide_alpha_mass { get; set; }
        private static double peptide_beta_mass { get; set; }
        private static double peptide_alpha_score { get; set; }
        private static double peptide_beta_score { get; set; }
        #endregion

        #region PPI
        private static ProteinProteinInteraction current_ppi;
        private static int ppi_fdr { get; set; }
        private static double ppi_score { get; set; }
        private static string gene_b { get; set; }
        private static string gene_a { get; set; }

        private static string[] HeaderLinePPI { get; set; }
        #endregion
        /// <summary>
        /// Method responsible for processing the header line
        /// </summary>
        /// <param name="row"></param>
        private static void ProcessHeaderCSM(string row)
        {
            HeaderLineCSM = Regex.Split(row, "\",");
            for (int i = 0; i < HeaderLineCSM.Length; i++)
            {
                HeaderLineCSM[i] = Regex.Replace(HeaderLineCSM[i], "\"", "");
            }
        }

        private static void ProcessHeaderPPI(string row)
        {
            HeaderLinePPI = Regex.Split(row, "\",");
            if (HeaderLinePPI.Length > 1)
            {
                for (int i = 0; i < HeaderLinePPI.Length; i++)
                {
                    HeaderLinePPI[i] = Regex.Replace(HeaderLinePPI[i], "\"", "");
                }

            }
            else
            {
                HeaderLinePPI = Regex.Split(row, ",");
            }
        }

        public static List<CSMSearchResult> ParseCSMs(string fileName, short fileIndex)
        {
            List<CSMSearchResult> myCSMs = new List<CSMSearchResult>();
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(fileName);
            }
            catch (Exception)
            {
                return myCSMs;
            }
            string line = "";
            int csm_processed = 0;
            int old_progress = 0;
            double lengthFile = File.ReadAllLines(fileName).Length;

            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > 0)
                {
                    if (line.StartsWith("\"index"))
                    {
                        ProcessHeaderCSM(line);
                    }
                    else
                    {
                        ProcessCSMs(line, fileIndex);
                        if (current_csm != null)
                        {
                            myCSMs.Add(current_csm);
                            current_csm = null;
                        }
                    }
                }
                csm_processed++;
                int new_progress = (int)((double)csm_processed / (lengthFile) * 100);
                if (new_progress > old_progress)
                {
                    old_progress = new_progress;
                    Console.Write("Reading XlinkX CSM File: " + old_progress + "%");
                }
            }

            sr.Close();
            return myCSMs;
        }

        /// <summary>
        /// Method responsible for processing each line file
        /// </summary>
        /// <param name="qtdParser"></param>
        private static void ProcessCSMs(string row, short fileIndex)
        {
            List<string> cols = new List<string>();
            string[] initial_cols = Regex.Split(row, "\",");

            for (int i = 0; i < initial_cols.Length; i++)
            {
                if (initial_cols[i].Contains("\""))
                {
                    int index_offset = initial_cols[i].IndexOf("\"");
                    string sub_string = initial_cols[i].Substring(0, index_offset);
                    if (!String.IsNullOrEmpty(sub_string))
                    {
                        string[] current = Regex.Split(sub_string, ",");
                        cols.AddRange(current);

                        sub_string = initial_cols[i].Substring(index_offset, initial_cols[i].Length - index_offset);
                        cols.Add(Regex.Replace(sub_string, "\"", ""));
                        cols.RemoveAll(a => String.IsNullOrEmpty(a));
                    }
                    else
                    {
                        cols.Add(Regex.Replace(initial_cols[i], "\"", ""));
                    }
                }
                else
                {
                    cols.AddRange(Regex.Split(initial_cols[i], ","));
                }
            }

            int index = Array.IndexOf(HeaderLineCSM, "index");
            if (index == -1) return;
            _index = cols[index];

            index = Array.IndexOf(HeaderLineCSM, "scan");
            if (index == -1) return;
            scanNumber = Convert.ToInt32(cols[index]);

            index = Array.IndexOf(HeaderLineCSM, "charge");
            if (index == -1) return;
            charge = Convert.ToInt32(cols[index]);

            index = Array.IndexOf(HeaderLineCSM, "precursor_mass");
            if (index == -1) return;
            precursor_mass = Convert.ToDouble(cols[index]);

            index = Array.IndexOf(HeaderLineCSM, "peptide_a");
            if (index == -1) return;
            peptide_alpha = cols[index];

            index = Array.IndexOf(HeaderLineCSM, "xl_a");
            if (index == -1) return;
            pep_alpha_pos = Convert.ToInt16(cols[index]);

            index = Array.IndexOf(HeaderLineCSM, "pep_pos_a");
            if (index == -1) return;
            ptn_alpha_pos = (short)(Convert.ToInt16(cols[index]) + pep_alpha_pos);

            index = Array.IndexOf(HeaderLineCSM, "protein_a");
            if (index == -1) return;
            protein_alpha = cols[index];

            index = Array.IndexOf(HeaderLineCSM, "mass_a");
            if (index == -1) return;
            peptide_alpha_mass = Convert.ToDouble(cols[index]);

            index = Array.IndexOf(HeaderLineCSM, "n_score_a_MS2_MS3");
            if (index == -1) return;
            peptide_alpha_score = Convert.ToDouble(cols[index]);

            index = Array.IndexOf(HeaderLineCSM, "peptide_b");
            if (index == -1) return;
            peptide_beta = cols[index];

            index = Array.IndexOf(HeaderLineCSM, "xl_b");
            if (index == -1) return;
            pep_beta_pos = Convert.ToInt16(cols[index]);

            index = Array.IndexOf(HeaderLineCSM, "pep_pos_b");
            if (index == -1) return;
            ptn_beta_pos = (short)(Convert.ToInt16(cols[index]) + pep_beta_pos);

            index = Array.IndexOf(HeaderLineCSM, "protein_b");
            if (index == -1) return;
            protein_beta = cols[index];

            index = Array.IndexOf(HeaderLineCSM, "mass_b");
            if (index == -1) return;
            peptide_beta_mass = Convert.ToDouble(cols[index]);

            index = Array.IndexOf(HeaderLineCSM, "n_score_b_MS2_MS3");
            if (index == -1) return;
            peptide_beta_score = Convert.ToDouble(cols[index]);

            string gene_alpha = "";
            string gene_beta = "";

            string[] gene_alpha_cols = Regex.Split(protein_alpha, " ");
            int _gene_alpha_index = Array.FindIndex(gene_alpha_cols, item => item.StartsWith("GN="));
            if (_gene_alpha_index != -1)
                gene_alpha = Regex.Split(gene_alpha_cols[_gene_alpha_index], "GN=")[1];

            string[] gene_beta_cols = Regex.Split(protein_beta, " ");
            int _gene_beta_index = Array.FindIndex(gene_beta_cols, item => item.StartsWith("GN="));
            if (_gene_beta_index != -1)
                gene_beta = Regex.Split(gene_beta_cols[_gene_beta_index], "GN=")[1];

            current_csm = new CSMSearchResult(_index, fileIndex, scanNumber, charge, precursor_mass, peptide_alpha, peptide_beta, pep_alpha_pos, pep_beta_pos, ptn_alpha_pos, ptn_beta_pos, new List<string>() { protein_alpha }, new List<string>() { protein_beta }, peptide_alpha_mass, peptide_beta_mass, peptide_alpha_score, peptide_beta_score, new List<string>() { gene_alpha }, new List<string>() { gene_beta });
        }
        
        public static List<ProteinProteinInteraction> ParsePPI(string fileName)
        {
            List<ProteinProteinInteraction> ppiList = new List<ProteinProteinInteraction>();
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(fileName);
            }
            catch (Exception)
            {
                return ppiList;
            }
            string line = "";
            int ppi_processed = 0;
            int old_progress = 0;
            double lengthFile = File.ReadAllLines(fileName).Length;
            bool isPPIFile = false;

            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > 0)
                {
                    if (line.StartsWith("\"gene_a") || line.StartsWith("gene_a"))
                    {
                        ProcessHeaderPPI(line);
                        isPPIFile = true;
                    }
                    else
                    {
                        if (!isPPIFile) break;
                        ProcessPPI(line, fileIndex);
                        if (current_ppi != null)
                        {
                            ppiList.Add(current_ppi);
                            current_ppi = null;
                        }
                    }
                }
                ppi_processed++;
                int new_progress = (int)((double)ppi_processed / (lengthFile) * 100);
                if (new_progress > old_progress)
                {
                    old_progress = new_progress;
                    Console.Write("Reading XlinkX PPI File: " + old_progress + "%");
                }
            }

            sr.Close();
            return ppiList;
        }

        private static void ProcessPPI(string row, short fileIndex)
        {
            List<string> cols = new List<string>();
            string[] initial_cols = Regex.Split(row, "\",");

            for (int i = 0; i < initial_cols.Length; i++)
            {
                if (initial_cols[i].Contains("\""))
                {
                    int index_offset = initial_cols[i].IndexOf("\"");
                    string sub_string = initial_cols[i].Substring(0, index_offset);
                    if (!String.IsNullOrEmpty(sub_string))
                    {
                        string[] current = Regex.Split(sub_string, ",");
                        cols.AddRange(current);

                        sub_string = initial_cols[i].Substring(index_offset, initial_cols[i].Length - index_offset);
                        cols.Add(Regex.Replace(sub_string, "\"", ""));
                        cols.RemoveAll(a => String.IsNullOrEmpty(a));
                    }
                    else
                    {
                        cols.Add(Regex.Replace(initial_cols[i], "\"", ""));
                    }
                }
                else
                {
                    cols.AddRange(Regex.Split(initial_cols[i], ","));
                }
            }

            int index = Array.IndexOf(HeaderLinePPI, "gene_a");
            if (index == -1) return;
            gene_a = cols[index];

            index = Array.IndexOf(HeaderLinePPI, "gene_b");
            if (index == -1) return;
            gene_b = cols[index];

            index = Array.IndexOf(HeaderLinePPI, "uniprot_a");
            if (index == -1) return;
            protein_alpha = cols[index];

            index = Array.IndexOf(HeaderLinePPI, "uniprot_b");
            if (index == -1) return;
            protein_beta = cols[index];

            index = Array.IndexOf(HeaderLinePPI, "score_cmb");
            if (index == -1) return;
            ppi_score = Convert.ToDouble(cols[index]);

            index = Array.IndexOf(HeaderLinePPI, "FDR");
            if (index != -1)
                ppi_fdr = Convert.ToInt32(cols[index]);

            current_ppi = new ProteinProteinInteraction(gene_a, gene_b, protein_alpha, protein_beta, ppi_score, ppi_fdr);
        }
    }
}
