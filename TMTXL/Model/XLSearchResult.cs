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
    public class XLSearchResult : CSMSearchResult
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
    }
}
