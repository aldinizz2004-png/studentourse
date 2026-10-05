using StudentCourseManagement.Repositories;

var studentRepository = new StudentRepository();
var courseRepository = new CourseRepository();
var enrollmentRepository = new EnrollmentRepository();

string currentSemester = "Semester 1";

while (true)
{
    Console.Clear();

    Console.WriteLine("==========================================");
    Console.WriteLine("     STUDENT COURSE MANAGEMENT SYSTEM");
    Console.WriteLine("==========================================");
    Console.WriteLine($"Current Semester: {currentSemester}");
    Console.WriteLine();

    Console.WriteLine("1. Add Student");
    Console.WriteLine("2. Add Course");
    Console.WriteLine("3. Enroll Student in Course");
    Console.WriteLine("4. Display Students");
    Console.WriteLine("5. Display Courses");
    Console.WriteLine("6. Display Enrollments");
    Console.WriteLine("7. Soft Delete Student");
    Console.WriteLine("8. Soft Delete Course");
    Console.WriteLine("9. Search Enrollments");
    Console.WriteLine("10. Change Current Semester");
    Console.WriteLine("0. Exit");

    Console.WriteLine();
    Console.Write("Choose an option: ");

    var choice = Console.ReadLine();

    Console.Clear();

    try
    {
        switch (choice)
        {
            case "1":
                await AddStudent();
                break;

            case "2":
                await AddCourse();
                break;

            case "3":
                await EnrollStudent();
                break;

            case "4":
                await DisplayStudents();
                break;

            case "5":
                await DisplayCourses();
                break;

            case "6":
                await DisplayEnrollments();
                break;

            case "7":
                await SoftDeleteStudent();
                break;

            case "8":
                await SoftDeleteCourse();
                break;

            case "9":
                await SearchEnrollments();
                break;

            case "10":
                ChangeSemester();
                break;

            case "0":
                Console.WriteLine("Goodbye.");
                return;

            default:
                Console.WriteLine("Invalid option.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine("An error occurred:");
        Console.WriteLine(ex.Message);
    }

    Console.WriteLine();
    Console.WriteLine("Press ENTER to continue...");
    Console.ReadLine();
}


// ==========================================
// ADD STUDENT
// ==========================================

async Task AddStudent()
{
    Console.WriteLine("=== Add Student ===");
    Console.WriteLine();

    Console.Write("Name: ");
    var name = Console.ReadLine()?.Trim();

    Console.Write("Email: ");
    var email = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Student name cannot be empty.");
        return;
    }

    if (string.IsNullOrWhiteSpace(email))
    {
        Console.WriteLine("Student email cannot be empty.");
        return;
    }

    var id = await studentRepository.AddStudentAsync(
        name,
        email
    );

    Console.WriteLine();
    Console.WriteLine($"Student created successfully. ID: {id}");
}


// ==========================================
// ADD COURSE
// ==========================================

async Task AddCourse()
{
    Console.WriteLine("=== Add Course ===");
    Console.WriteLine();

    Console.Write("Course name: ");
    var name = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Course name cannot be empty.");
        return;
    }

    var id = await courseRepository.AddCourseAsync(name);

    Console.WriteLine();
    Console.WriteLine($"Course created successfully. ID: {id}");
}


// ==========================================
// ENROLL STUDENT
// ==========================================

async Task EnrollStudent()
{
    Console.WriteLine("=== Enroll Student ===");
    Console.WriteLine($"Semester: {currentSemester}");
    Console.WriteLine();

    await DisplayStudents();

    Console.WriteLine();

    if (!ReadInt("Student ID: ", out var studentId))
        return;

    Console.WriteLine();

    await DisplayCourses();

    Console.WriteLine();

    if (!ReadInt("Course ID: ", out var courseId))
        return;

    Console.WriteLine();

    var result =
        await enrollmentRepository.EnrollStudentAsync(
            studentId,
            courseId,
            currentSemester
        );

    Console.WriteLine(result);
}


// ==========================================
// DISPLAY STUDENTS
// ==========================================

async Task DisplayStudents()
{
    var students =
        await studentRepository.GetAllStudentsAsync();

    Console.WriteLine("=== Students ===");
    Console.WriteLine();

    Console.WriteLine(
        $"{"ID",-5} {"Name",-20} {"Email",-30} {"Status",-10}"
    );

    Console.WriteLine(new string('-', 70));

    foreach (var student in students)
    {
        var status =
            student.IsActive
                ? "Active"
                : "Inactive";

        Console.WriteLine(
            $"{student.Id,-5} " +
            $"{student.Name,-20} " +
            $"{student.Email,-30} " +
            $"{status,-10}"
        );
    }
}


// ==========================================
// DISPLAY COURSES
// ==========================================

async Task DisplayCourses()
{
    var courses =
        await courseRepository.GetAllCoursesAsync();

    Console.WriteLine("=== Courses ===");
    Console.WriteLine();

    Console.WriteLine(
        $"{"ID",-5} {"Course",-30} {"Status",-10}"
    );

    Console.WriteLine(new string('-', 50));

    foreach (var course in courses)
    {
        var status =
            course.IsActive
                ? "Active"
                : "Inactive";

        Console.WriteLine(
            $"{course.Id,-5} " +
            $"{course.Name,-30} " +
            $"{status,-10}"
        );
    }
}


// ==========================================
// DISPLAY ENROLLMENTS
// ==========================================

async Task DisplayEnrollments()
{
    var enrollments =
        await enrollmentRepository.GetEnrollmentsAsync();

    Console.WriteLine("=== All Enrollments ===");
    Console.WriteLine();

    var list = enrollments.ToList();

    if (list.Count == 0)
    {
        Console.WriteLine("No enrollments found.");
        return;
    }

    Console.WriteLine(
        $"{"Student",-20} {"Course",-30} {"Semester",-15}"
    );

    Console.WriteLine(new string('-', 70));

    foreach (var enrollment in list)
    {
        Console.WriteLine(
            $"{enrollment.StudentName,-20} " +
            $"{enrollment.CourseName,-30} " +
            $"{enrollment.Semester,-15}"
        );
    }
}


// ==========================================
// SOFT DELETE STUDENT
// ==========================================

async Task SoftDeleteStudent()
{
    Console.WriteLine("=== Soft Delete Student ===");
    Console.WriteLine();

    await DisplayStudents();

    Console.WriteLine();

    if (!ReadInt("Student ID: ", out var studentId))
        return;

    Console.WriteLine();
    Console.WriteLine(
        $"Student will become inactive."
    );

    Console.WriteLine(
        $"Enrollments from {currentSemester} will be deleted."
    );

    Console.WriteLine(
        "Enrollments from previous semesters will remain."
    );

    Console.WriteLine();

    Console.Write("Continue? (y/n): ");

    var confirm = Console.ReadLine();

    if (!string.Equals(
            confirm,
            "y",
            StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Operation cancelled.");
        return;
    }

    var result =
        await studentRepository.SoftDeleteStudentAsync(
            studentId,
            currentSemester
        );

    Console.WriteLine();

    Console.WriteLine(
        result
            ? "Student deactivated successfully."
            : "Student not found or already inactive."
    );
}


// ==========================================
// SOFT DELETE COURSE
// ==========================================

async Task SoftDeleteCourse()
{
    Console.WriteLine("=== Soft Delete Course ===");
    Console.WriteLine();

    await DisplayCourses();

    Console.WriteLine();

    if (!ReadInt("Course ID: ", out var courseId))
        return;

    Console.WriteLine();
    Console.WriteLine(
        $"Course will become inactive."
    );

    Console.WriteLine(
        $"Enrollments from {currentSemester} will be deleted."
    );

    Console.WriteLine(
        "Enrollments from previous semesters will remain."
    );

    Console.WriteLine();

    Console.Write("Continue? (y/n): ");

    var confirm = Console.ReadLine();

    if (!string.Equals(
            confirm,
            "y",
            StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Operation cancelled.");
        return;
    }

    var result =
        await courseRepository.SoftDeleteCourseAsync(
            courseId,
            currentSemester
        );

    Console.WriteLine();

    Console.WriteLine(
        result
            ? "Course deactivated successfully."
            : "Course not found or already inactive."
    );
}


// ==========================================
// GENERAL / FLEXIBLE QUERY
// ==========================================

async Task SearchEnrollments()
{
    Console.WriteLine("=== Search Enrollments ===");
    Console.WriteLine();

    Console.WriteLine("Leave any field empty to ignore it.");
    Console.WriteLine();

    Console.Write("Student name: ");
    var studentName = Console.ReadLine()?.Trim();

    Console.Write("Course name: ");
    var courseName = Console.ReadLine()?.Trim();

    Console.Write("Semester: ");
    var semester = Console.ReadLine()?.Trim();

    Console.Write("Active students/courses only? (y/n): ");

    var activeInput = Console.ReadLine();

    var activeOnly =
        string.Equals(
            activeInput,
            "y",
            StringComparison.OrdinalIgnoreCase
        );

    var results =
        await enrollmentRepository.GetEnrollmentsAsync(
            studentName:
                string.IsNullOrWhiteSpace(studentName)
                    ? null
                    : studentName,

            courseName:
                string.IsNullOrWhiteSpace(courseName)
                    ? null
                    : courseName,

            semester:
                string.IsNullOrWhiteSpace(semester)
                    ? null
                    : semester,

            activeOnly: activeOnly
        );

    var list = results.ToList();

    Console.WriteLine();

    if (list.Count == 0)
    {
        Console.WriteLine("No matching enrollments found.");
        return;
    }

    Console.WriteLine(
        $"{"Student",-20} {"Course",-30} {"Semester",-15}"
    );

    Console.WriteLine(new string('-', 70));

    foreach (var enrollment in list)
    {
        Console.WriteLine(
            $"{enrollment.StudentName,-20} " +
            $"{enrollment.CourseName,-30} " +
            $"{enrollment.Semester,-15}"
        );
    }
}


// ==========================================
// CHANGE CURRENT SEMESTER
// ==========================================

void ChangeSemester()
{
    Console.WriteLine("=== Change Current Semester ===");
    Console.WriteLine();

    Console.WriteLine($"Current: {currentSemester}");

    Console.Write("New semester: ");

    var semester = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(semester))
    {
        Console.WriteLine("Semester cannot be empty.");
        return;
    }

    currentSemester = semester;

    Console.WriteLine();
    Console.WriteLine(
        $"Current semester changed to: {currentSemester}"
    );
}


// ==========================================
// INTEGER INPUT HELPER
// ==========================================

bool ReadInt(
    string message,
    out int value)
{
    Console.Write(message);

    var input = Console.ReadLine();

    if (!int.TryParse(input, out value))
    {
        Console.WriteLine("Invalid number.");
        return false;
    }

    return true;
}