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
            string applicantsFolderPath = args[3];
            string selectedFolderPath = args[4];
            string waitlistedFolderPath = args[5];

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
                school.Applicants.Shuffle(new Random());
                schoolLottery.Run(school,school.Applicants,false); // false because RIDE wants to output the randomized Applicant list, so doing this before. Randomization done on prior line.
            }
            reconciler.Reconcile(schools);

            //TODO Move this to the Ripl.Csv package
            // Write to CSV files
            foreach (School school in schools)
            {
                // Randomized applicant list
                string randomizedApplicantsFilePath = applicantsFolderPath + "\\" + school.Name + ".csv";
                using (StreamWriter textWriter = new StreamWriter(randomizedApplicantsFilePath))
                {
                    CsvWriter csvWriter = new CsvWriter(textWriter);
                    csvWriter.WriteRecords(school.WaitlistedApplicants);
                }
                
                // Selected
                string selectedFilePath = selectedFolderPath + "\\" + school.Name + ".csv"; 
                using (StreamWriter textWriter = new StreamWriter(selectedFilePath))
                {
                    CsvWriter csvWriter = new CsvWriter(textWriter);
                    csvWriter.WriteRecords(school.SelectedApplicants);
                }

                // Wait List
                string waitListedFilePath = waitlistedFolderPath + "\\" + school.Name + ".csv"; 
                using (StreamWriter textWriter = new StreamWriter(waitListedFilePath))
                {
                    CsvWriter csvWriter = new CsvWriter(textWriter);
                    csvWriter.WriteRecords(school.WaitlistedApplicants);
                }
            }

            using(StreamWriter textWriter = new StreamWriter(selectedFolderPath + "\\" + "summary.txt"))
            {
                foreach(School s in schools.OrderBy(s=>s.Name))
                {
                    textWriter.WriteLine("************");
                    textWriter.WriteLine("School: {0}", s.Name);

                    textWriter.WriteLine("Applied: {0} (includes duplicates, out-of-district)", s.Applicants.Count());

                    int a_belowPovertyCount = s.Applicants.Where(v => v.IsBelowPovertyLevel).Count();
                    textWriter.WriteLine("\tBelow Poverty: {0} (includes duplicates, out-of-district)", a_belowPovertyCount);

                    int a_abovePovertyCount = s.Applicants.Where(v => !v.IsBelowPovertyLevel).Count();
                    textWriter.WriteLine("\tAbove Poverty: {0} (includes duplicates, out-of-district)", a_abovePovertyCount);

                    int a_numBoys = s.Applicants.Where(v => v.StudentGender == Applicant.Gender.MALE).Count();
                    textWriter.WriteLine("\tBoys: {0} (includes duplicates, out-of-district)", a_numBoys);

                    int a_numGirls = s.Applicants.Where(v => v.StudentGender == Applicant.Gender.FEMALE).Count();
                    textWriter.WriteLine("\tGirls: {0} (includes duplicates, out-of-district)", a_numGirls);

                    textWriter.WriteLine("Selected: {0}", s.SelectedApplicants.Count());
                
                    int s_belowPovertyCount = s.SelectedApplicants.Where(v => v.IsBelowPovertyLevel).Count();
                    textWriter.WriteLine("\tBelow Poverty: {0}", s_belowPovertyCount);
                
                    int s_abovePovertyCount = s.SelectedApplicants.Where(v => !v.IsBelowPovertyLevel).Count();
                    textWriter.WriteLine("\tAbove Poverty: {0}", s_abovePovertyCount);

                    int s_numBoys = s.SelectedApplicants.Where(v => v.StudentGender == Applicant.Gender.MALE).Count();
                    textWriter.WriteLine("\tBoys: {0}", s_numBoys);

                    int s_numGirls = s.SelectedApplicants.Where(v => v.StudentGender == Applicant.Gender.FEMALE).Count();
                    textWriter.WriteLine("\tGirls: {0}", s_numGirls);

                    textWriter.WriteLine("Waitlisted: {0}", s.WaitlistedApplicants.Count());

                    int w_belowPovertyCount = s.WaitlistedApplicants.Where(v => v.IsBelowPovertyLevel).Count();
                    textWriter.WriteLine("\tBelow Poverty: {0}", w_belowPovertyCount);

                    int w_abovePovertyCount = s.WaitlistedApplicants.Where(v => !v.IsBelowPovertyLevel).Count();
                    textWriter.WriteLine("\tAbove Poverty: {0}", w_abovePovertyCount);

                    int w_numBoys = s.WaitlistedApplicants.Where(v => v.StudentGender == Applicant.Gender.MALE).Count();
                    textWriter.WriteLine("\tBoys: {0}", w_numBoys);

                    int w_numGirls = s.WaitlistedApplicants.Where(v => v.StudentGender == Applicant.Gender.FEMALE).Count();
                    textWriter.WriteLine("\tGirls: {0}", w_numGirls);

                }
            }
        }
    }
}
