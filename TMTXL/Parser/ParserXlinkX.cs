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
        private static CSMSearchResult current_csm;
        private static string[] HeaderLine { get; set; }
        private static string _index { get; set; }
        private static short fileIndex { get; set; }
        private static int scanNumber { get; set; }
        private static int charge { get; set; }
        private static double precursor_mass { get; set; }
        private static string peptide_alpha { get; set; }
        private static string peptide_beta { get; set; }
        private static short pos_alpha { get; set; }
        private static short pos_beta { get; set; }
        private static string protein_alpha { get; set; }
        private static string protein_beta { get; set; }
        private static double peptide_alpha_mass { get; set; }
        private static double peptide_beta_mass { get; set; }
        private static double peptide_alpha_score { get; set; }
        private static double peptide_beta_score { get; set; }

        /// <summary>
        /// Method responsible for processing the header line
        /// </summary>
        /// <param name="row"></param>
        private static void ProcessHeader(string row)
        {
            HeaderLine = Regex.Split(row, "\",");
            for (int i = 0; i < HeaderLine.Length; i++)
            {
                HeaderLine[i] = Regex.Replace(HeaderLine[i], "\"", "");
            }
        }

        public static List<CSMSearchResult> Parse(string fileName, short fileIndex)
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
                        ProcessHeader(line);
                    }
                    else
                    {
                        Process(line, fileIndex);
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
                    Console.Write("Reading XlinkX File: " + old_progress + "%");
                }
            }

            sr.Close();
            return myCSMs;
        }

        /// <summary>
        /// Method responsible for processing each line file
        /// </summary>
        /// <param name="qtdParser"></param>
        private static void Process(string row, short fileIndex)
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
                        cols.RemoveAll(a=> String.IsNullOrEmpty(a));
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

            int index = Array.IndexOf(HeaderLine, "index");
            if (index == -1) return;
            _index = cols[index];

            index = Array.IndexOf(HeaderLine, "scan");
            if (index == -1) return;
            scanNumber = Convert.ToInt32(cols[index]);

            index = Array.IndexOf(HeaderLine, "charge");
            if (index == -1) return;
            charge = Convert.ToInt32(cols[index]);

            index = Array.IndexOf(HeaderLine, "precursor_mass");
            if (index == -1) return;
            precursor_mass = Convert.ToDouble(cols[index]);

            index = Array.IndexOf(HeaderLine, "peptide_a");
            if (index == -1) return;
            peptide_alpha = cols[index];

            index = Array.IndexOf(HeaderLine, "xl_a");
            if (index == -1) return;
            pos_alpha = Convert.ToInt16(cols[index]);

            index = Array.IndexOf(HeaderLine, "protein_a");
            if (index == -1) return;
            protein_alpha = cols[index];

            index = Array.IndexOf(HeaderLine, "mass_a");
            if (index == -1) return;
            peptide_alpha_mass = Convert.ToDouble(cols[index]);

            index = Array.IndexOf(HeaderLine, "n_score_a_MS2_MS3");
            if (index == -1) return;
            peptide_alpha_score = Convert.ToDouble(cols[index]);

            index = Array.IndexOf(HeaderLine, "peptide_b");
            if (index == -1) return;
            peptide_beta = cols[index];

            index = Array.IndexOf(HeaderLine, "xl_b");
            if (index == -1) return;
            pos_beta = Convert.ToInt16(cols[index]);

            index = Array.IndexOf(HeaderLine, "protein_b");
            if (index == -1) return;
            protein_beta = cols[index];

            index = Array.IndexOf(HeaderLine, "mass_b");
            if (index == -1) return;
            peptide_beta_mass = Convert.ToDouble(cols[index]);

            index = Array.IndexOf(HeaderLine, "n_score_b_MS2_MS3");
            if (index == -1) return;
            peptide_beta_score = Convert.ToDouble(cols[index]);

            current_csm = new CSMSearchResult(_index, fileIndex, scanNumber, charge, precursor_mass, peptide_alpha, peptide_beta, pos_alpha, pos_beta, protein_alpha, protein_beta, peptide_alpha_mass, peptide_beta_mass, peptide_alpha_score, peptide_beta_score);
        }
    }
}
