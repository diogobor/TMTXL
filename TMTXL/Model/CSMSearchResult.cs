using PatternTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTXL.Model
{
    /// <summary>
    /// Model class for XlinkX results (csms)
    /// </summary>
    public class CSMSearchResult
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public CSMSearchResult() { }

        public string _index { get; set; }
        public short fileIndex { get; set; }
        public int scanNumber { get; set; }
        public int charge { get; set; }
        public double precursor_mass { get; set; }
        public string peptide_alpha { get; set; }
        public string peptide_beta { get; set; }
        public short pos_alpha { get; set; }
        public short pos_beta { get; set; }
        public string protein_alpha { get; set; }
        public string protein_beta { get; set; }
        public double peptide_alpha_mass { get; set; }
        public double peptide_beta_mass { get; set; }
        public double peptide_alpha_score { get; set; }
        public double peptide_beta_score { get; set; }
        public string fileName { get; set; }

        public List<double> quantitation { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileIndex"></param>
        /// <param name="scanNumber"></param>
        /// <param name="charge"></param>
        /// <param name="precursor_mass"></param>
        /// <param name="peptide_alpha"></param>
        /// <param name="peptide_beta"></param>
        /// <param name="pos_alpha"></param>
        /// <param name="pos_beta"></param>
        /// <param name="protein_alpha"></param>
        /// <param name="protein_beta"></param>
        /// <param name="peptide_alpha_mass"></param>
        /// <param name="peptide_beta_mass"></param>
        /// <param name="peptide_alpha_score"></param>
        /// <param name="peptide_beta_score"></param>
        public CSMSearchResult(string index, short fileIndex, int scanNumber, int charge, double precursor_mass, string peptide_alpha, string peptide_beta, short pos_alpha, short pos_beta, string protein_alpha, string protein_beta, double peptide_alpha_mass, double peptide_beta_mass, double peptide_alpha_score, double peptide_beta_score)
        {
            _index = index;
            this.fileIndex = fileIndex;
            this.scanNumber = scanNumber;
            this.charge = charge;
            this.precursor_mass = precursor_mass;
            this.peptide_alpha = peptide_alpha;
            this.peptide_beta = peptide_beta;
            this.pos_alpha = pos_alpha;
            this.pos_beta = pos_beta;
            this.protein_alpha = protein_alpha;
            this.protein_beta = protein_beta;
            this.peptide_alpha_mass = peptide_alpha_mass;
            this.peptide_beta_mass = peptide_beta_mass;
            this.peptide_alpha_score = peptide_alpha_score;
            this.peptide_beta_score = peptide_beta_score;
        }
    }
}
