﻿using Ripl.Model;
using System;
using System.Collections.Generic;
using ListShuffleExtensions;

namespace Ripl.Lottery
{
    public class SchoolLottery
    {
        int studentsPerClassroom;
        double percentMale;
        DateTime age4ByDate;

        public SchoolLottery(int studentsPerClassroom, double percentMale, DateTime age4ByDate)
        {
            this.studentsPerClassroom = studentsPerClassroom;
            this.percentMale = percentMale;
            this.age4ByDate = age4ByDate;
        }

        public School Run(School school)
        {
            return Run(school, school.Applicants);
        }

        public School Run(School school, List<Applicant> applicantList)
        {
            // Counts
            int countMale = 0;
            int countFemale = 0;
            int countBelowPovertyLine = 0;
            int countAbovePovertyLine = 0;
            
            // Initial calculations
            int numStudents = school.NumClassrooms * studentsPerClassroom;
            int numMale = (int)Math.Round(numStudents * percentMale);
            int numFemale = numStudents - numMale;
            int numBelowPovertyLine = (int)Math.Round(numStudents*school.PercentBelowPovertyLine);
            int numAbovePovertyLine = numStudents - numBelowPovertyLine;

            // Copy the list to preserve it
            List<Applicant> applicants = new List<Applicant>(applicantList);

            // Remove duplicates
            applicants = RemoveDuplicates(applicants);
            //TODO Maybe add the removed duplicates to the school's list

            // Remove those that don't live in the distrct
            applicants = FilterByDistrict(applicants,school.District);

            // Remove applicants who are too old or young
            applicants = FilterByAge(applicants);

            // Randomly sort the list
            //TODO This should work great, but double check anyway
            applicants.Shuffle(new Random());

            // Select low income students
            List<Applicant> lowIncomeApplicants = GetByPovertyStatus(applicants, true);
            foreach(Applicant a in lowIncomeApplicants)
            {
                // If the low income quota has been met, move on
                if(countBelowPovertyLine >= numBelowPovertyLine || school.SelectedApplicants.Count >= numStudents)
                {
                    break;
                }

                //TODO check on this
                // Add the student if the male/female ratio hasn't been violated
                if(a.StudentGender == Applicant.Gender.MALE && countMale < numMale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countBelowPovertyLine++;
                    countMale++;
                }
                else if(a.StudentGender == Applicant.Gender.FEMALE && countFemale < numFemale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countBelowPovertyLine++;
                    countFemale++;
                }
            }

            // Select higher income students
            // TODO refactor -- almost the same as the above loop
            List<Applicant> higherIncomeApplicants = GetByPovertyStatus(applicants, false);
            foreach (Applicant a in higherIncomeApplicants)
            {
                // If the higher income quota has been met, move on
                if (countAbovePovertyLine >= numAbovePovertyLine || school.SelectedApplicants.Count >= numStudents)
                {
                    break;
                }

                //TODO check on this
                // Add the student if the male/female ratio hasn't been violated
                if (a.StudentGender == Applicant.Gender.MALE && countMale < numMale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countAbovePovertyLine++;
                    countMale++;
                }
                else if (a.StudentGender == Applicant.Gender.FEMALE && countFemale < numFemale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countAbovePovertyLine++;
                    countFemale++;
                }
            }

            // Are there still openings? (income agnostic, gender checked selection)
            foreach(Applicant a in new List<Applicant>(applicants)) // prevents modification during iteration
            {
                if(school.SelectedApplicants.Count >= numStudents)
                {
                    break;
                }

                //TODO check on this
                //TODO refactor -- this chunk of code is similar to other male/female selections
                // Add the student if the male/female ratio hasn't been violated
                if (a.StudentGender == Applicant.Gender.MALE && countMale < numMale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countMale++;
                }
                else if (a.StudentGender == Applicant.Gender.FEMALE && countFemale < numFemale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countFemale++;
                }
            }

            // Are there still openings? (income and gender agnostic selection)
            foreach (Applicant a in new List<Applicant>(applicants))
            {
                if (school.SelectedApplicants.Count >= numStudents)
                {
                    break;
                }

                school.SelectedApplicants.Add(a);
                applicants.Remove(a);
            }

            // Wait list the rest
            school.WaitlistedApplicants.AddRange(applicants);

            return school;
        }

        private List<Applicant> RemoveDuplicates(List<Applicant> applicants)
        {
            List<Applicant> dedupedApplicants = new List<Applicant>();
            HashSet<string> applicantCodes = new HashSet<string>();

            foreach(Applicant a in applicants)
            {
                string code = a.StudentFirstName + a.StudentMiddleName + a.StudentLastName + a.StreetAddress + a.District + a.ZipCode;
                if(!applicantCodes.Contains(code))
                {
                    dedupedApplicants.Add(a);
                    applicantCodes.Add(code);
                }
            }

            return dedupedApplicants;
        }

        private static List<Applicant> FilterByDistrict(List<Applicant> applicants, string district)
        {
            List<Applicant> filteredApplicants = new List<Applicant>();

            foreach (Applicant a in applicants)
            {
                if(district.Equals(a.District))
                {
                    filteredApplicants.Add(a);
                }
            }

            return filteredApplicants;
        }

        private List<Applicant> FilterByAge(List<Applicant> applicants)
        {
            List<Applicant> filteredApplicants = new List<Applicant>();

            foreach (Applicant a in applicants)
            {
                int ageByCutoff = age4ByDate.Year - a.StudentBirthday.Year;
                DateTime adjustedDate = age4ByDate.AddYears(-ageByCutoff);
                if(a.StudentBirthday > adjustedDate)
                {
                    ageByCutoff--;
                }

                if (ageByCutoff == 4)
                {
                    filteredApplicants.Add(a);
                }
            }

            return filteredApplicants;
        }

        private List<Applicant> GetByPovertyStatus(List<Applicant> applicants, bool isBelowPovertyLine)
        {
            List<Applicant> filteredApplicants = new List<Applicant>();
            foreach(Applicant a in applicants)
            {
                if(a.IsBelowPovertyLevel == isBelowPovertyLine)
                {
                    filteredApplicants.Add(a);
                }
            }

            return filteredApplicants;
        }
    }
}

// Used from http://stackoverflow.com/a/22668974/249016
namespace ListShuffleExtensions
{
    public static class ListShuffleExtensions
    {
        public static void Shuffle<T>(this IList<T> list, Random rnd)
        {
            for (var i = 0; i < list.Count; i++)
                list.Swap(i, rnd.Next(i, list.Count));
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
