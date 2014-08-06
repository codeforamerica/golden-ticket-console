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

            RunLottery(schoolConfigPath, incomeConfigPath, applicantCsvPath);
        }

        //TODO cleanup -- works fine, just sloppy
        public static void RunLottery(string schoolConfigPath, string incomeConfigPath, string applicantCsvPath)
        {
            // Read in school configurations
            SchoolReader schoolReader = new SchoolCsvReader(schoolConfigPath);
            List<School> schools = schoolReader.ReadSchools();

            // Read in applicants and split across schools applied for
            IncomeReader incomeReader = new IncomeCsvReader(incomeConfigPath);
            IncomeCalculator incomeCalc = new IncomeCalculator(incomeReader);

            ApplicantReader applicantReader = new ApplicantCsvReader(applicantCsvPath, schools, incomeCalc);
            applicantReader.ReadApplicants();

            // Perform the lottery for each school
            var settings = ConfigurationManager.AppSettings;
            int numStudentsPerClassroom = int.Parse(settings["numStudentsPerClassroom"]);
            double percentMale = double.Parse(settings["percentMale"]);
            DateTime age4ByDate = Convert.ToDateTime(settings["age4ByDate"]);
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
                string selectedFilePath = "C:\\temp\\selected\\" + school.Name + ".csv"; //TODO use arguments
                using (StreamWriter textWriter = new StreamWriter(selectedFilePath))
                {
                    CsvWriter csvWriter = new CsvWriter(textWriter);
                    csvWriter.WriteRecords(schoolRun.SelectedApplicants);
                }

                // Wait List
                string waitListedFilePath = "C:\\temp\\wait\\" + school.Name + ".csv"; //TODO use arguments
                using (StreamWriter textWriter = new StreamWriter(waitListedFilePath))
                {
                    CsvWriter csvWriter = new CsvWriter(textWriter);
                    csvWriter.WriteRecords(schoolRun.WaitlistedApplicants);
                }
            }
        }
}
