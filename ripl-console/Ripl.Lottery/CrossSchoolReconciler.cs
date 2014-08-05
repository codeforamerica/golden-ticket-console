using Ripl.Model;
using System;
using System.Collections.Generic;
using Ripl.Lottery; //for List shuffle extensions


namespace Ripl.Lottery
{
    public class CrossSchoolReconciler
    {
        SchoolLottery schoolLottery;

        public CrossSchoolReconciler(SchoolLottery schoolLottery)
        {
            this.schoolLottery = schoolLottery;
        }

        //TODO This can be made more efficient
        public List<School> Reconcile(List<School> schools)
        {
            // Remove selected students from all school waitlists
            RemoveSelectedFromWaitlists(schools);

            // Reconcile students selected for multiple schools
            // i.e. Leave the student in the school in which they are highest on the selected list
            var reconciledApplicants = new List<Applicant>();
            var remainingSchools = new List<School>(schools);
            foreach(School s in schools)
            {
                schoolLoop: { }
                List<Applicant> selectedApplicants = new List<Applicant>(s.SelectedApplicants); // to prevent concurrent modification during iteration
                foreach(Applicant a in selectedApplicants)
                {
                    // Skip applicant if already reconciled
                    if(reconciledApplicants.Contains(a))
                    {
                        continue;
                    }

                    // Reconcile the applicant between schools he/she was selected 
                    List<School> selectedSchools = GetSchoolsApplicantWasSelectedAt(a, remainingSchools);
                    bool effectsCurrentSchool = ReconcileApplicant(a, selectedSchools, s);

                    // Adjust the control structures
                    reconciledApplicants.Add(a);

                    // Reset the loop if the current has been impacted
                    if (effectsCurrentSchool)
                    {
                        goto schoolLoop; //TODO Is there a better way?
                    }
                }
                remainingSchools.Remove(s);
            }

            return schools;
        }

        //TODO is there a way to do this with a function pointer or delegate on selected?
        private Dictionary<string,HashSet<string>> MakeChecksumsForSelected(IEnumerable<School> schools)
        {
            var checksums = new Dictionary<string, HashSet<string>>();

            foreach(School s in schools)
            {
                checksums[s.Name] = new HashSet<string>();
                foreach(Applicant a in s.SelectedApplicants)
                {
                    checksums[s.Name].Add(a.Checksum());
                }
            }

            return checksums;
        }

        //TODO is there a way to do this with a function pointer or delegate on waitlisted?
        private Dictionary<string, HashSet<string>> MakeChecksumsForWaitlisted(IEnumerable<School> schools)
        {
            var checksums = new Dictionary<string, HashSet<string>>();

            foreach (School s in schools)
            {
                checksums[s.Name] = new HashSet<string>();
                foreach (Applicant a in s.WaitlistedApplicants)
                {
                    checksums[s.Name].Add(a.Checksum());
                }
            }

            return checksums;
        }

        private List<Applicant> GetAllSelectedApplicants(IEnumerable<School> schools)
        {
            List<Applicant> selectedApplicants = new List<Applicant>();
            foreach(School s in schools)
            {
                selectedApplicants.AddRange(s.SelectedApplicants);
            }
            return selectedApplicants;
        }

        //TODO make this more efficient
        private void RemoveSelectedFromWaitlists(IEnumerable<School> schools)
        {
            List<Applicant> allSelectedApplicants = GetAllSelectedApplicants(schools);
            foreach(Applicant a in allSelectedApplicants)
            {
                foreach(School s in schools)
                {
                    s.WaitlistedApplicants.Remove(a);
                }
            }
        }

        private List<School> GetSchoolsApplicantWasSelectedAt(Applicant applicant, IEnumerable<School> schools)
        {
            List<School> selectedSchools = new List<School>();
            
            foreach(School s in schools)
            {
                if(s.SelectedApplicants.Contains(applicant))
                {
                    selectedSchools.Add(s);
                }
            }

            return selectedSchools;
        }

        private bool ReconcileApplicant(Applicant applicant, List<School> schools, School currentSchool)
        {
            // If applicant is only in one school, no reconciliation needed
            if(schools.Count <= 1)
            {
                return false;
            }

            // Determine which school the applicant is the lowest (number-wise) on the list
            School lowestSchool = null;
            int lowestIndex = 10000; //start very high
            
            List<School> shuffledSchools = new List<School>(schools);
            shuffledSchools.Shuffle(new Random());

            foreach(School s in shuffledSchools)
            {
                int index = s.SelectedApplicants.IndexOf(applicant);
                if(index < lowestIndex)
                {
                    lowestSchool = s;
                    lowestIndex = index;
                }
            }

            // Remove the student from the higher (number-wise) on the list schools and run lotteries for those schools
            bool effectsCurrentSchool = false;
            foreach(School s in schools)
            {
                if(s != lowestSchool)
                {
                    s.SelectedApplicants.Remove(applicant);
                    schoolLottery.Run(s, s.WaitlistedApplicants);
                    if(s == currentSchool)
                    {
                        effectsCurrentSchool = true;
                    }
                }
            }

            return effectsCurrentSchool;
        }
    }
}
