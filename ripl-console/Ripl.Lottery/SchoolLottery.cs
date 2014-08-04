using Ripl.Model;
using System;
using System.Collections.Generic;

namespace Ripl.Lottery
{
    public class SchoolLottery
    {
        int studentsPerClassroom;
        double percentMale;

        public SchoolLottery(int studentsPerClassroom, double percentMale)
        {
            this.studentsPerClassroom = studentsPerClassroom;
            this.percentMale = percentMale;
        }

        public School Run(School school)
        {
            return null;
        }
    }
}
