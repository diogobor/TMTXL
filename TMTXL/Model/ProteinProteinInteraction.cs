using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTXL.Model
{
    /// <summary>
    /// Model class for XlinkX results (ppi)
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class ProteinProteinInteraction
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public ProteinProteinInteraction() { }

        [ProtoMember(1)]
        public string gene_a { get; set; }
        [ProtoMember(2)]
        public string gene_b { get; set; }
        [ProtoMember(3)]
        public string protein_a { get; set; }
        [ProtoMember(4)]
        public string protein_b { get; set; }
        [ProtoMember(5)]
        public double score { get; set; }
        [ProtoMember(6)]
        public double fdr { get; set; }
        [ProtoMember(7)]
        public List<double> quantitation { get; set; }
        [ProtoMember(8)]
        public int totalCSM { get; set; }
        [ProtoMember(9)]
        public int uniqueCSM { get; set; }
        [ProtoMember(10)]
        public int specCount;
        [ProtoMember(11)]
        public double log2FoldChange;
        [ProtoMember(12)]
        public double pValue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gene_a"></param>
        /// <param name="gene_b"></param>
        /// <param name="protein_a"></param>
        /// <param name="protein_b"></param>
        /// <param name="score"></param>
        /// <param name="fdr"></param>
        /// <param name="quantitation"></param>
        /// <param name="totalCSM"></param>
        /// <param name="uniqueCSM"></param>
        public ProteinProteinInteraction(string gene_a, string gene_b, string protein_a, string protein_b, double score, double fdr)
        {
            this.gene_a = gene_a;
            this.gene_b = gene_b;
            this.protein_a = protein_a;
            this.protein_b = protein_b;
            this.score = score;
            this.fdr = fdr;
        }
    }
}
