using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTXL.Model
{
    /// <summary>
    /// Model class for XlinkX results (xlms)
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class XLSearchResult : CSMSearchResult, IEquatable<XLSearchResult>
    {
        [ProtoMember(1)]
        public List<CSMSearchResult> cSMs { get; set; }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public XLSearchResult() { }

        public XLSearchResult(List<CSMSearchResult> cSMs)
        {
            this.cSMs = cSMs;
        }

        /// <summary>
        /// Method responsible for cloning object
        /// </summary>
        /// <returns></returns>
        public XLSearchResult ShallowCopy()
        {
            XLSearchResult xlsr = (XLSearchResult)this.MemberwiseClone();
            xlsr.cSMs = new List<CSMSearchResult>();
            this.cSMs.ForEach(a =>
            {
                CSMSearchResult csm = a.ShallowCopy();
                xlsr.cSMs.Add(csm);
            });
            return xlsr;
        }

        public bool Equals(XLSearchResult other)
        {
            return this.cSMs != null && other.cSMs != null && this.cSMs.Count == other.cSMs.Count && this.cSMs.SequenceEqual(other.cSMs) &&
                this.log2FoldChange != null && other.log2FoldChange != null && this.log2FoldChange.Count == other.log2FoldChange.Count && this.log2FoldChange.SequenceEqual(other.log2FoldChange) &&
                this.pValue != null && other.pValue != null && this.pValue.Count == other.pValue.Count && this.pValue.SequenceEqual(other.pValue);
        }
    }
}
