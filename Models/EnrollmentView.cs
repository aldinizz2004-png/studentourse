namespace StudentCourseManagement.Models;

public class EnrollmentView
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string StudentEmail { get; set; } = string.Empty;

    public bool StudentIsActive { get; set; }

    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public bool CourseIsActive { get; set; }

    public string Semester { get; set; } = string.Empty;
}