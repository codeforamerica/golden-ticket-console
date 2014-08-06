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
            string selectedFolderPath = args[3];
            string waitlistedFolderPath = args[4];

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
                var schoolRun = schoolLottery.Run(school);

                // Selected
                string selectedFilePath = selectedFolderPath + "\\" + school.Name + ".csv"; 
                using (StreamWriter textWriter = new StreamWriter(selectedFilePath))
                {
                    CsvWriter csvWriter = new CsvWriter(textWriter);
                    csvWriter.WriteRecords(schoolRun.SelectedApplicants);
                }

                // Wait List
                string waitListedFilePath = waitlistedFolderPath + "\\" + school.Name + ".csv"; 
                using (StreamWriter textWriter = new StreamWriter(waitListedFilePath))
                {
                    CsvWriter csvWriter = new CsvWriter(textWriter);
                    csvWriter.WriteRecords(schoolRun.WaitlistedApplicants);
                }
            }

            using(StreamWriter textWriter = new StreamWriter(selectedFolderPath + "\\" + "summary.txt"))
            {
                foreach(School s in schools.OrderBy(s=>s.Name))
                {
                    textWriter.WriteLine("************");
                    textWriter.WriteLine("School: {0}", s.Name);
                    textWriter.WriteLine("Selected: {0}", s.SelectedApplicants.Count());
                
                    int belowPovertyCount = s.SelectedApplicants.Where(v => v.IsBelowPovertyLevel).Count();
                    textWriter.WriteLine("\tBelow Poverty: {0}", belowPovertyCount);
                
                    int abovePovertyCount = s.SelectedApplicants.Where(v => !v.IsBelowPovertyLevel).Count();
                    textWriter.WriteLine("\tAbove Poverty: {0}", abovePovertyCount);

                    int numBoys = s.SelectedApplicants.Where(v => v.StudentGender == Applicant.Gender.MALE).Count();
                    textWriter.WriteLine("\tBoys: {0}", numBoys);

                    int numGirls = s.SelectedApplicants.Where(v => v.StudentGender == Applicant.Gender.FEMALE).Count();
                    textWriter.WriteLine("\tGirls: {0}", numGirls);

                    textWriter.WriteLine("Waitlisted: {0}", s.WaitlistedApplicants.Count());

                    belowPovertyCount = s.WaitlistedApplicants.Where(v => v.IsBelowPovertyLevel).Count();
                    textWriter.WriteLine("\tBelow Poverty: {0}", belowPovertyCount);

                    abovePovertyCount = s.WaitlistedApplicants.Where(v => !v.IsBelowPovertyLevel).Count();
                    textWriter.WriteLine("\tAbove Poverty: {0}", abovePovertyCount);

                    numBoys = s.WaitlistedApplicants.Where(v => v.StudentGender == Applicant.Gender.MALE).Count();
                    textWriter.WriteLine("\tBoys: {0}", numBoys);

                    numGirls = s.WaitlistedApplicants.Where(v => v.StudentGender == Applicant.Gender.FEMALE).Count();
                    textWriter.WriteLine("\tGirls: {0}", numGirls);

                }
            }
        }
    }
}
