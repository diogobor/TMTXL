using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTXL.Control
{
    [Serializable]
    [ProtoContract]
    public class ProgramParams
    {
        [ProtoMember(1)]
        public string RawFilesDir { get;  set; }
        [ProtoMember(2)]
        public string IDdir { get;  set; }
        [ProtoMember(3)]
        public List<List<double>> PurityCorrectionMatrix { get; set; }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public ProgramParams() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="RawFilesDir"></param>
        /// <param name="IDdir"></param>
        public ProgramParams(string RawFilesDir,string IDdir)
        {
            this.RawFilesDir = RawFilesDir;
            this.IDdir = IDdir;
        }
    }
}
