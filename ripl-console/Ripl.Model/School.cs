using System;
using System.Collections.Generic;

namespace Ripl.Model
{
    public class School
    {
        string name;
        string district;
        double percentBelowPovertyLine;
        int numClassrooms;

        List<Applicant> applicants = new List<Applicant>();
        List<Applicant> selectedApplicants = new List<Applicant>();
        List<Applicant> waitlistedApplicants = new List<Applicant>();
        List<Applicant> duplicateApplicants = new List<Applicant>();

        public School()
        {
        }

        public School(string name, string district)
        {
            this.name = name;
            this.district = district;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string District
        {
            get { return district; }
            set { district = value; }
        }

        public double PercentBelowPovertyLine
        {
            get { return percentBelowPovertyLine; }
            set { percentBelowPovertyLine = value; }
        }

        public int NumClassrooms
        {
            get { return numClassrooms; }
            set { numClassrooms = value; }
        }

        public List<Applicant> Applicants
        {
            get { return applicants; }
        }

        public List<Applicant> SelectedApplicants
        {
            get { return selectedApplicants; }
        }

        public List<Applicant> WaitlistedApplicants
        {
            get { return waitlistedApplicants; }
            set { waitlistedApplicants = value; }
        }

        public List<Applicant> DuplicateApplicants
        {
            get { return duplicateApplicants; }
            set { duplicateApplicants = value; }
        }

        public void ClearApplicants()
        {
            applicants.Clear();
            selectedApplicants.Clear();
            waitlistedApplicants.Clear();
            duplicateApplicants.Clear();
        }
    }
}
