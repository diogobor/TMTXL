using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uniprot.Model
{
    public class Protein
    {
        public string ProteinID { get; set; }
        public string AccessionNumber { get; set; }
        public string Sequence { get; set; }
        public short DBPtnLength { get; set; }
        public short ProteinLength { get; set; }

        public Protein(string proteinID, string accessionNumber, string sequence, short proteinLength)
        {
            this.ProteinID = proteinID;
            this.AccessionNumber = accessionNumber;
            this.Sequence = sequence;
            this.DBPtnLength = proteinLength;
        }
    }

    public class ProteinComparer : IEqualityComparer<Protein>
    {
        public bool Equals(Protein x, Protein y)
        {
            //Check if the compared objects reference has same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check if any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check if the items' properties are equal.
            return x.ProteinID.Equals(y.ProteinID) &&
                x.AccessionNumber.Equals(y.AccessionNumber) &&
                x.Sequence.Equals(y.Sequence) &&
                x.DBPtnLength == y.DBPtnLength &&
                x.ProteinLength == y.ProteinLength;
        }

        public int GetHashCode(Protein obj)
        {
            return obj.AccessionNumber.GetHashCode() * 17 + obj.ProteinID.Length;
        }
    }

}
