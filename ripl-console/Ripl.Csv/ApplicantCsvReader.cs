using CsvHelper;
using Ripl.Model;
using Ripl.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ripl.Csv
{
    public class ApplicantCsvReader : ApplicantReader
    {
        string csvFilePath;
        Dictionary<string,School> schools;

        public ApplicantCsvReader(string csvFilePath, List<School> schoolList)
        {
            this.csvFilePath = csvFilePath;

            schools = new Dictionary<string,School>();
            schoolList.ForEach(s => schools[s.Name] = s);
        }

        public List<School> ReadApplicants()
        {
            // Clear the schools
            foreach(string key in schools.Keys)
            {
                schools[key].ClearApplicants();
            }

            // Read the CSV
            using (StreamReader textReader = new StreamReader(csvFilePath))
            {
                CsvReader csvReader = new CsvReader(textReader);
                csvReader.Configuration.SkipEmptyRecords = true;
                csvReader.Configuration.TrimFields = true;

                while (csvReader.Read())
                {
                    Applicant applicant = ParseApplicant(csvReader);

                    // Add student to each school
                    string schoolNames = csvReader.GetField<string>("Schools");
                    foreach (string rawSchoolName in schoolNames.Split(','))
                    {
                        string schoolName = EscapeSchoolName(rawSchoolName.Trim());
                        schools[schoolName].Applicants.Add(applicant);
                    }
                }
            }

            return schools.Values.ToList();
        }

        private static Applicant ParseApplicant(CsvReader csvReader)
        {
            Applicant a = new Applicant();

            a.StudentFirstName = csvReader.GetField<string>("Student First Name");
            a.StudentMiddleName = csvReader.GetField<string>("Student Middle Name");
            a.StudentLastName = csvReader.GetField<string>("Student Last Name");
            a.StudentBirthday = Convert.ToDateTime(csvReader.GetField<string>("Student Birthday"));
            a.StudentGender = ParseGender(csvReader.GetField<string>("Student Gender"));

            a.GuardianFirstName = csvReader.GetField<string>("Guardian First Name");
            a.GuardianLastName = csvReader.GetField<string>("Guardian Last Name");
            a.GuardianPhoneNumber = csvReader.GetField<string>("Guardian Phone Number");
            a.GuardianEmailAddress = csvReader.GetField<string>("Guardian E-mail Address");
            a.GuardianRelationshipToStudent = csvReader.GetField<string>("Guardian Relationship to Student");

            a.StreetAddress = csvReader.GetField<string>("Street Address");
            a.ZipCode = csvReader.GetField<string>("Zip Code");
            a.District = csvReader.GetField<string>("District of Residency");
            a.NumHouseholdMembers = int.Parse(csvReader.GetField<string>("Household Members"));
            a.AnnualHouseholdIncomeRange = csvReader.GetField<string>("Annual Income");
            a.AnnualHouseholdIncome = csvReader.GetField<string>("Household Income Amount");

            return a;
        }

        private static string EscapeSchoolName(string schoolName)
        {
            return schoolName.Replace("@", "at").Replace('/', '-');
        }

        private static Applicant.Gender ParseGender(string genderStr)
        {
            if (genderStr.Equals("MALE", StringComparison.InvariantCultureIgnoreCase))
            {
                return Applicant.Gender.MALE;
            }

            return Applicant.Gender.FEMALE;
        }
    }
}
