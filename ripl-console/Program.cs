using Ripl.Reader;
using Ripl.Csv;
using Ripl.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ripl
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }

        //*********************************************
        // Throw away temp methods
        //*********************************************

        public static void ReadIncome()
        {
            IncomeReader incomeReader = new IncomeCsvReader("C:\\Users\\jeff\\code\\ripl-console\\ripl-console\\samples\\config\\income.csv");
            var incomeLevels = incomeReader.ReadIncome();

            foreach(int numHouseholdMembers in incomeLevels.Keys)
            {
                Console.WriteLine("{0} --> {1}", numHouseholdMembers, incomeLevels[numHouseholdMembers]);
            }
        }

        public static void ReadApplicants()
        {
            SchoolReader schoolReader = new SchoolCsvReader("c:\\users\\jeff\\code\\ripl-console\\ripl-console\\samples\\config\\schools.csv");
            List<School> schools = schoolReader.ReadSchools();

            ApplicantReader applicantReader = new ApplicantCsvReader("C:\\temp\\data.csv", schools);
            applicantReader.ReadApplicants();

            foreach(School school in schools)
            {
                Console.WriteLine("***************");
                Console.WriteLine("School: {0}", school.Name);
                
                foreach(Applicant applicant in school.Applicants)
                {
                    Console.WriteLine(" - {0} {1}", applicant.StudentFirstName, applicant.StudentLastName);
                }
            }
        }

        public static void ReadSchoolSettings()
        {
            SchoolReader schoolReader = new SchoolCsvReader("c:\\users\\jeff\\code\\ripl-console\\ripl-console\\samples\\config\\schools.csv");
            List<School> schools = schoolReader.ReadSchools();

            foreach(School school in schools)
            {
                Console.WriteLine("**************************");
                Console.WriteLine("Name: {0}", school.Name);
                Console.WriteLine("Classrooms: {0}", school.NumClassrooms);
                Console.WriteLine("District: {0}", school.District);
                Console.WriteLine("Percent Below Poverty Line: {0}", school.PercentBelowPovertyLine);
            }
        }

        public static void ReadAppSettings()
        {
            var settings = ConfigurationManager.AppSettings;
            int numStudentsPerClassroom = int.Parse(settings["numStudentsPerClassroom"]);
            Console.WriteLine(numStudentsPerClassroom);
        }
    }
}
