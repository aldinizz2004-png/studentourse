using Microsoft.Data.SqlClient;

namespace StudentCourseManagement.Data;

public static class Database
{
    private const string ConnectionString =
        "Server=localhost\\SQLEXPRESS01;Database=StudentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection CreateConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}