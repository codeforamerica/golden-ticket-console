using Ripl.Model;
using System;
using System.Collections.Generic;

namespace Ripl.Reader
{
    public interface ApplicantReader
    {
        List<School> ReadApplicants();
    }
}
