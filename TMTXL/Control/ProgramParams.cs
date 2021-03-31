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
        public List<List<double>> PurityCorrectionMatrix { get; set; }
        [ProtoMember(3)]
        public string ClassLabels { get; set; }
        [ProtoMember(4)]
        public List<double> IsobaricMassess { get; set; }
        [ProtoMember(5)]
        ///0: None; 1: MS2-MS2; 2: SPS-MS3
        public short Multinoch { get; set; }
        [ProtoMember(6)]
        public string ChemicalLabel { get; set; }
        [ProtoMember(7)]
        public int ControlChannel { get; set; }
        [ProtoMember(8)]
        public bool NormalizeSpectra { get; set; }
        

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
