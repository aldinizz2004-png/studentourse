using Dapper;
using StudentCourseManagement.Data;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repositories;

public class StudentRepository
{
    public async Task<int> AddStudentAsync(string name, string email)
    {
        using var connection = Database.CreateConnection();

        const string sql = """
            INSERT INTO Students (Name, Email)
            VALUES (@Name, @Email);

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        var studentId = await connection.QuerySingleAsync<int>(
            sql,
            new
            {
                Name = name,
                Email = email
            }
        );

        return studentId;
    }

    public async Task<IEnumerable<Student>> GetAllStudentsAsync()
    {
        using var connection = Database.CreateConnection();

        const string sql = """
            SELECT Id, Name, Email, IsActive
            FROM Students
            ORDER BY Id;
            """;

        return await connection.QueryAsync<Student>(sql);
    }

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        using var connection = Database.CreateConnection();

        const string sql = """
            SELECT Id, Name, Email, IsActive
            FROM Students
            WHERE Id = @Id;
            """;

        return await connection.QuerySingleOrDefaultAsync<Student>(
            sql,
            new { Id = id }
        );
    }

    public async Task<bool> SoftDeleteStudentAsync(
        int studentId,
        string currentSemester)
    {
        using var connection = Database.CreateConnection();

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string deactivateStudentSql = """
                UPDATE Students
                SET IsActive = 0
                WHERE Id = @StudentId
                  AND IsActive = 1;
                """;

            var affectedRows = await connection.ExecuteAsync(
                deactivateStudentSql,
                new
                {
                    StudentId = studentId
                },
                transaction
            );

            if (affectedRows == 0)
            {
                transaction.Rollback();
                return false;
            }

            const string deleteCurrentEnrollmentsSql = """
                DELETE FROM Enrollments
                WHERE StudentId = @StudentId
                  AND Semester = @CurrentSemester;
                """;

            await connection.ExecuteAsync(
                deleteCurrentEnrollmentsSql,
                new
                {
                    StudentId = studentId,
                    CurrentSemester = currentSemester
                },
                transaction
            );

            transaction.Commit();

            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}