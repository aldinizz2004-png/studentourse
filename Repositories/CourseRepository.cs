using Dapper;
using StudentCourseManagement.Data;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repositories;

public class CourseRepository
{
    public async Task<int> AddCourseAsync(string name)
    {
        using var connection = Database.CreateConnection();

        const string sql = """
            INSERT INTO Courses (Name)
            VALUES (@Name);

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        return await connection.QuerySingleAsync<int>(
            sql,
            new { Name = name }
        );
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        using var connection = Database.CreateConnection();

        const string sql = """
            SELECT Id, Name, IsActive
            FROM Courses
            ORDER BY Id;
            """;

        return await connection.QueryAsync<Course>(sql);
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        using var connection = Database.CreateConnection();

        const string sql = """
            SELECT Id, Name, IsActive
            FROM Courses
            WHERE Id = @Id;
            """;

        return await connection.QuerySingleOrDefaultAsync<Course>(
            sql,
            new { Id = id }
        );
    }

    public async Task<bool> SoftDeleteCourseAsync(
        int courseId,
        string currentSemester)
    {
        using var connection = Database.CreateConnection();

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string deactivateCourseSql = """
                UPDATE Courses
                SET IsActive = 0
                WHERE Id = @CourseId
                  AND IsActive = 1;
                """;

            var affectedRows = await connection.ExecuteAsync(
                deactivateCourseSql,
                new
                {
                    CourseId = courseId
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
                WHERE CourseId = @CourseId
                  AND Semester = @CurrentSemester;
                """;

            await connection.ExecuteAsync(
                deleteCurrentEnrollmentsSql,
                new
                {
                    CourseId = courseId,
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