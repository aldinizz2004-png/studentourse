namespace StudentCourseManagement.Models;

public class Enrollment
{
    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public string Semester { get; set; } = string.Empty;
}