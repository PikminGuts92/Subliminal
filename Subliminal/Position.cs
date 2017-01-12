using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subliminal
{
    public class Position
    {
        public int Row;
        public int Column;

        public Position() : this(0, 0)
        {

        }

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public override string ToString()
        {
            return string.Format("Row: {0} Column: {1}", Row, Column);
        }

        public override int GetHashCode()
        {
            unchecked // Means it's okay to overflow
            {
                const int prime = 23;
                int hash = 59; // Also a prime

                hash *= prime + Row.GetHashCode();
                hash *= prime + Column.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }
    }
}
