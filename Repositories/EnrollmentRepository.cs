using System.Text;
using Dapper;
using StudentCourseManagement.Data;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repositories;

public class EnrollmentRepository
{
    public async Task<string> EnrollStudentAsync(
        int studentId,
        int courseId,
        string semester)
    {
        using var connection = Database.CreateConnection();

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string studentSql = """
                SELECT IsActive
                FROM Students
                WHERE Id = @StudentId;
                """;

            var studentIsActive =
                await connection.QuerySingleOrDefaultAsync<bool?>(
                    studentSql,
                    new { StudentId = studentId },
                    transaction
                );

            if (studentIsActive is null)
            {
                transaction.Rollback();
                return "Student does not exist.";
            }

            if (studentIsActive == false)
            {
                transaction.Rollback();
                return "Inactive student cannot be enrolled.";
            }


            const string courseSql = """
                SELECT IsActive
                FROM Courses
                WHERE Id = @CourseId;
                """;

            var courseIsActive =
                await connection.QuerySingleOrDefaultAsync<bool?>(
                    courseSql,
                    new { CourseId = courseId },
                    transaction
                );

            if (courseIsActive is null)
            {
                transaction.Rollback();
                return "Course does not exist.";
            }

            if (courseIsActive == false)
            {
                transaction.Rollback();
                return "Inactive course cannot receive students.";
            }


            const string duplicateSql = """
                SELECT COUNT(*)
                FROM Enrollments
                WHERE StudentId = @StudentId
                  AND CourseId = @CourseId
                  AND Semester = @Semester;
                """;

            var duplicateCount =
                await connection.ExecuteScalarAsync<int>(
                    duplicateSql,
                    new
                    {
                        StudentId = studentId,
                        CourseId = courseId,
                        Semester = semester
                    },
                    transaction
                );

            if (duplicateCount > 0)
            {
                transaction.Rollback();
                return "Student is already enrolled in this course for this semester.";
            }


            const string studentCourseCountSql = """
                SELECT COUNT(*)
                FROM Enrollments
                WHERE StudentId = @StudentId
                  AND Semester = @Semester;
                """;

            var studentCourseCount =
                await connection.ExecuteScalarAsync<int>(
                    studentCourseCountSql,
                    new
                    {
                        StudentId = studentId,
                        Semester = semester
                    },
                    transaction
                );

            if (studentCourseCount >= 3)
            {
                transaction.Rollback();
                return "Student cannot enroll in more than 3 courses per semester.";
            }


            const string courseStudentCountSql = """
                SELECT COUNT(*)
                FROM Enrollments
                WHERE CourseId = @CourseId
                  AND Semester = @Semester;
                """;

            var courseStudentCount =
                await connection.ExecuteScalarAsync<int>(
                    courseStudentCountSql,
                    new
                    {
                        CourseId = courseId,
                        Semester = semester
                    },
                    transaction
                );

            if (courseStudentCount >= 8)
            {
                transaction.Rollback();
                return "Course cannot have more than 8 students per semester.";
            }


            const string insertSql = """
                INSERT INTO Enrollments
                (
                    StudentId,
                    CourseId,
                    Semester
                )
                VALUES
                (
                    @StudentId,
                    @CourseId,
                    @Semester
                );
                """;

            await connection.ExecuteAsync(
                insertSql,
                new
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    Semester = semester
                },
                transaction
            );

            transaction.Commit();

            return "Enrollment completed successfully.";
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


    public async Task<IEnumerable<EnrollmentView>> GetEnrollmentsAsync(
        int? studentId = null,
        string? studentName = null,
        int? courseId = null,
        string? courseName = null,
        string? semester = null,
        bool activeOnly = false)
    {
        using var connection = Database.CreateConnection();

        var sql = new StringBuilder("""
            SELECT
                e.StudentId,
                s.Name AS StudentName,
                s.Email AS StudentEmail,
                s.IsActive AS StudentIsActive,

                e.CourseId,
                c.Name AS CourseName,
                c.IsActive AS CourseIsActive,

                e.Semester

            FROM Enrollments e

            INNER JOIN Students s
                ON s.Id = e.StudentId

            INNER JOIN Courses c
                ON c.Id = e.CourseId

            WHERE 1 = 1
            """);

        var parameters = new DynamicParameters();


        if (studentId.HasValue)
        {
            sql.AppendLine("AND e.StudentId = @StudentId");
            parameters.Add("StudentId", studentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(studentName))
        {
            sql.AppendLine("AND s.Name = @StudentName");
            parameters.Add("StudentName", studentName);
        }

        if (courseId.HasValue)
        {
            sql.AppendLine("AND e.CourseId = @CourseId");
            parameters.Add("CourseId", courseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(courseName))
        {
            sql.AppendLine("AND c.Name = @CourseName");
            parameters.Add("CourseName", courseName);
        }

        if (!string.IsNullOrWhiteSpace(semester))
        {
            sql.AppendLine("AND e.Semester = @Semester");
            parameters.Add("Semester", semester);
        }

        if (activeOnly)
        {
            sql.AppendLine("""
                AND s.IsActive = 1
                AND c.IsActive = 1
                """);
        }

        sql.AppendLine("""
            ORDER BY
                e.Semester,
                s.Name,
                c.Name;
            """);

        return await connection.QueryAsync<EnrollmentView>(
            sql.ToString(),
            parameters
        );
    }
}