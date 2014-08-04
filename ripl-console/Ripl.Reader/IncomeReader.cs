using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ripl.Reader
{
    public interface IncomeReader
    {
        Dictionary<int, int> ReadIncome();
    }
}
