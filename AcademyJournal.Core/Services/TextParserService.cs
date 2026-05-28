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

            string[] firstSplitInput = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<StudentGradeInput> output = new List<StudentGradeInput>();

            foreach (string line in firstSplitInput)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;

                List<string> splitInput = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (splitInput.Count < 3)
                {
                    throw new Exception("InvalidFormatException"); // Неверный формат строки
                }

                string name = splitInput[0] + " " + splitInput[1];

                List<int> grades = new List<int>();
                for (int i = 2; i < splitInput.Count; i++)
                {
                    if (int.TryParse(splitInput[i], out int gradeValue))
                    {
                        // Валидация оценок из вашего исходного кода
                        if (gradeValue > 12)
                        {
                            throw new Exception("GradeGreaterThan12");
                        }
                        if (gradeValue <= 0)
                        {
                            throw new Exception("GradeLessOrEqual0");
                        }
                        grades.Add(gradeValue);
                    }
                    else
                    {
                        throw new Exception("NotANumberException");
                    }
                }

                output.Add(new StudentGradeInput(name, grades));

            }

            return output;
        }
    }

}
