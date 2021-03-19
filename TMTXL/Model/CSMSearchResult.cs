using ProtoBuf;
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
    [Serializable]
    [ProtoContract]
    [ProtoInclude(26, typeof(XLSearchResult))]
    public class CSMSearchResult
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public CSMSearchResult() { }


        [ProtoMember(1)]
        public string _index { get; set; }
        [ProtoMember(2)]
        public short fileIndex { get; set; }
        [ProtoMember(3)]
        public int scanNumber { get; set; }
        [ProtoMember(4)]
        public int charge { get; set; }
        [ProtoMember(5)]
        public double precursor_mass { get; set; }
        [ProtoMember(6)]
        public string alpha_peptide { get; set; }
        [ProtoMember(7)]
        public string beta_peptide { get; set; }
        [ProtoMember(8)]
        public short alpha_pos_xl { get; set; }
        [ProtoMember(9)]
        public short beta_pos_xl { get; set; }
        [ProtoMember(10)]
        public short alpha_pept_xl_pos { get; set; }
        [ProtoMember(11)]
        public short beta_pept_xl_pos { get; set; }

        [ProtoMember(12)]
        public List<string> proteins_alpha { get; set; }
        [ProtoMember(13)]
        public List<string> proteins_beta { get; set; }
        [ProtoMember(14)]
        public double peptide_alpha_mass { get; set; }
        [ProtoMember(15)]
        public double peptide_beta_mass { get; set; }
        [ProtoMember(16)]
        public double peptide_alpha_score { get; set; }
        [ProtoMember(17)]
        public double peptide_beta_score { get; set; }
        [ProtoMember(18)]
        public string fileName { get; set; }
        [ProtoMember(19)]
        public List<double> quantitation { get; set; }
        [ProtoMember(20)]
        public List<string> genes_alpha { get; set; }
        [ProtoMember(21)]
        public List<string> genes_beta { get; set; }
        [ProtoMember(22)]
        public double avg_notNull_1 { get; set; }
        [ProtoMember(23)]
        public double avg_notNull_2 { get; set; }
        [ProtoMember(24)]
        public double log2FoldChange { get; set; }
        [ProtoMember(25)]
        public double pValue { get; set; }

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
        /// <param name="alpha_pept_xl_pos"></param>
        /// <param name="beta_pept_xl_pos"></param>
        /// <param name="proteins_alpha"></param>
        /// <param name="proteins_beta"></param>
        /// <param name="peptide_alpha_mass"></param>
        /// <param name="peptide_beta_mass"></param>
        /// <param name="peptide_alpha_score"></param>
        /// <param name="peptide_beta_score"></param>
        /// <param name="genes_alpha"></param>
        /// <param name="genes_beta"></param>
        public CSMSearchResult(string index, short fileIndex, int scanNumber, int charge, double precursor_mass, string peptide_alpha, string peptide_beta, short pos_alpha, short pos_beta, short alpha_pept_xl_pos, short beta_pept_xl_pos, List<string> proteins_alpha, List<string> proteins_beta, double peptide_alpha_mass, double peptide_beta_mass, double peptide_alpha_score, double peptide_beta_score, List<string> genes_alpha, List<string> genes_beta)
        {
            _index = index;
            this.fileIndex = fileIndex;
            this.scanNumber = scanNumber;
            this.charge = charge;
            this.precursor_mass = precursor_mass;
            this.alpha_peptide = peptide_alpha;
            this.beta_peptide = peptide_beta;
            this.alpha_pos_xl = pos_alpha;
            this.beta_pos_xl = pos_beta;
            this.alpha_pept_xl_pos = alpha_pept_xl_pos;
            this.beta_pept_xl_pos = beta_pept_xl_pos;
            this.proteins_alpha = proteins_alpha;
            this.proteins_beta = proteins_beta;
            this.peptide_alpha_mass = peptide_alpha_mass;
            this.peptide_beta_mass = peptide_beta_mass;
            this.peptide_alpha_score = peptide_alpha_score;
            this.peptide_beta_score = peptide_beta_score;
            this.genes_alpha = genes_alpha;
            this.genes_beta = genes_beta;
        }
    }
}
