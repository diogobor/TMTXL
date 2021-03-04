using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTXL.Model
{
    [Serializable]
    [ProtoContract]
    public class MassSpectrum
    {
        [ProtoMember(1)]
        public short ActivationType;
        [ProtoMember(2)]
        public double ChromatographyRetentionTime;
        [ProtoMember(3)]
        public short FileNameIndex;
        [ProtoMember(4)]
        public short InstrumentType;
        [ProtoMember(5)]
        public List<(double MZ, double Intensity)> Ions;
        [ProtoMember(6)]
        public short MSLevel;
        [ProtoMember(7)]
        public List<(double MZ, short Z)> Precursors;
        [ProtoMember(8)]
        public int ScanNumber;
        [ProtoMember(9)]
        public string ScanHeader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ActivationType"></param>
        /// <param name="ChromatographyRetentionTime"></param>
        /// <param name="FileNameIndex"></param>
        /// <param name="InstrumentType"></param>
        /// <param name="Ions"></param>
        /// <param name="MSLevel"></param>
        /// <param name="Precursors"></param>
        /// <param name="ScanNumber"></param>
        public MassSpectrum(short ActivationType,
            double ChromatographyRetentionTime,
            short FileNameIndex,
            short InstrumentType,
            List<(double MZ, double Intensity)> Ions,
            short MSLevel,
            List<(double MZ, short Z)> Precursors,
            int ScanNumber)
        {
            this.ActivationType = ActivationType;
            this.ChromatographyRetentionTime = ChromatographyRetentionTime;
            this.FileNameIndex = FileNameIndex;
            this.InstrumentType = InstrumentType;
            this.Ions = Ions;
            this.MSLevel = MSLevel;
            this.Precursors = Precursors;
            this.ScanNumber = ScanNumber;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public MassSpectrum() { }
    }
}
