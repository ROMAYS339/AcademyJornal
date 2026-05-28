using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyJournal.Core.Services
{
    public class StudentGradeInput
    {
        public string fullName { get; set; }

        public List<int> grades { get; set; }

        public StudentGradeInput(string name, List<int> _grades)
        {
            fullName = name;
            grades = _grades;
        }
    }

    public class TextParserService
    {
        static public List<StudentGradeInput> Parse(string input)
        {

            List<string> firstSplitInput = input.Split(Environment.NewLine).ToList();
            List<StudentGradeInput> output = new List<StudentGradeInput>();

            foreach (string line in firstSplitInput)
            {
                List<string> splitInput = line.Split(' ').ToList();
                List<string> gradesInput = new List<string>();

                for (int i = 2; i < splitInput.Count; i++)
                {
                    gradesInput.Add(splitInput[i]);
                }

                string name = splitInput[0] + " " + splitInput[1];

                List<int> grades = new List<int>();
                foreach (string s in gradesInput)
                {
                    if (int.TryParse(s, out int i))
                    {
                        grades.Add(i);
                        if (i > 12)
                        {
                            throw new Exception("GradeGreaterThan12");
                        }
                        if (i < 0)
                        {
                            throw new Exception("GradeLess0");
                        }
                    }
                    else
                    {
                        throw new Exception("NotANumberException");
                    }

                }

                StudentGradeInput studentGradeInput = new StudentGradeInput(name, grades);
                output.Add(studentGradeInput);

            }

            return output;
        }
    }

}
