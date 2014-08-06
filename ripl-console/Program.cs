using Ripl.Reader;
using Ripl.Csv;
using Ripl.Lottery;
using Ripl.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ripl.Calc;
using System.IO;
using CsvHelper;

namespace Ripl
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string schoolConfigPath = args[0];
            string incomeConfigPath = args[1];
            string applicantCsvPath = args[2];
            string rawApplicantsFolderPath = args[3];
            string filteredApplicantsFolderPath = args[4];
            string shuffledApplicantsFolderPath = args[5];
            string selectedFolderPath = args[6];
            string waitlistedFolderPath = args[7];

            // Read in school configurations
            SchoolReader schoolReader = new SchoolCsvReader(schoolConfigPath);
            List<School> schools = schoolReader.ReadSchools();

            // Read in applicants and split across schools applied for
            IncomeReader incomeReader = new IncomeCsvReader(incomeConfigPath);
            IncomeCalculator incomeCalc = new IncomeCalculator(incomeReader);

            ApplicantReader applicantReader = new ApplicantCsvReader(applicantCsvPath, schools, incomeCalc);
            applicantReader.ReadApplicants();

            // Perform the lottery for each school
            //var settings = ConfigurationManager.AppSettings; //TODO get App.config to be built into the EXE, until then settings are here directly
            int numStudentsPerClassroom = 18;
            double percentMale = 0.5;
            DateTime age4ByDate = Convert.ToDateTime("2014-09-01");
            
            SchoolLottery schoolLottery = new SchoolLottery(numStudentsPerClassroom, percentMale, age4ByDate);
            CrossSchoolReconciler reconciler = new CrossSchoolReconciler(schoolLottery);

            foreach (School school in schools)
            {
                schoolLottery.Run(school);
            }
            reconciler.Reconcile(schools);

            //TODO Move this to the Ripl.Csv package
            // Write to CSV files
            foreach (School school in schools)
            {
                // Raw applicant list
                WriteApplicantCsvFile(school.Applicants, rawApplicantsFolderPath + "\\" + school.Name + "_raw.csv");
                
                // Filtered
                WriteApplicantCsvFile(school.FilteredApplicants, filteredApplicantsFolderPath + "\\" + school.Name + "_filtered.csv");

                // Shuffled
                WriteApplicantCsvFile(school.ShuffledApplicants, shuffledApplicantsFolderPath + "\\" + school.Name + "_shuffled.csv");

                // Selected
                WriteApplicantCsvFile(school.SelectedApplicants, selectedFolderPath + "\\" + school.Name + "_selected.csv");

                // Waitlist
                WriteApplicantCsvFile(school.WaitlistedApplicants, waitlistedFolderPath + "\\" + school.Name + "_wailisted.csv");
            }

            using(StreamWriter textWriter = new StreamWriter(selectedFolderPath + "\\" + "summary.txt"))
            {
                foreach(School s in schools.OrderBy(s=>s.Name))
                {
                    textWriter.WriteLine("************");
                    textWriter.WriteLine("School: {0}", s.Name);
                    textWriter.WriteLine("\tPoverty Rate: {0}", s.PercentBelowPovertyLine);
                    textWriter.WriteLine("\tNumber of Classrooms (each w/ {1} students): {0}", s.NumClassrooms, numStudentsPerClassroom);

                    WriteSummaryGroup(textWriter, "Applied (pre-filter)", s.Applicants);
                    WriteSummaryGroup(textWriter, "Applied (post-filter)", s.FilteredApplicants);
                    WriteSummaryGroup(textWriter, "Selected", s.SelectedApplicants);
                    WriteSummaryGroup(textWriter, "Waitlisted", s.WaitlistedApplicants);
                }
            }
        }

        private static void WriteApplicantCsvFile(List<Applicant> applicants, string filePath)
        {
            using (StreamWriter textWriter = new StreamWriter(filePath))
            {
                CsvWriter csvWriter = new CsvWriter(textWriter);
                csvWriter.WriteRecords(applicants);
            }
        }

        private static void WriteSummaryGroup(StreamWriter textWriter, string groupName, List<Applicant> applicants)
        {
            textWriter.WriteLine("{0}: {1}", groupName, applicants.Count());

            int belowPovertyCount = applicants.Where(v => v.IsBelowPovertyLevel).Count();
            textWriter.WriteLine("\tBelow Poverty: {0}", belowPovertyCount);

            int abovePovertyCount = applicants.Where(v => !v.IsBelowPovertyLevel).Count();
            textWriter.WriteLine("\tAbove Poverty: {0}", abovePovertyCount);

            int numBoys = applicants.Where(v => v.StudentGender == Applicant.Gender.MALE).Count();
            textWriter.WriteLine("\tBoys: {0}", numBoys);

            int numGirls = applicants.Where(v => v.StudentGender == Applicant.Gender.FEMALE).Count();
            textWriter.WriteLine("\tGirls: {0}", numGirls);
        }
    }
}
