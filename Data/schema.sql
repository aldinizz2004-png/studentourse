CREATE DATABASE StudentManagementDB;
GO

USE StudentManagementDB;

CREATE TABLE Students (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    IsActive BIT NOT NULL 
     CONSTRAINT DF_Students_IsActive DEFAULT 1
    
    CONSTRAINT UF_Students_Email UNIQUE (Email)
);
GO 

CREATE TABLE Courses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    IsActive BIT NOT NULL 
     CONSTRAINT DF_Courses_IsActive DEFAULT 1
);
GO

CREATE TABLE Enrollments (
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    Semester NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_Enrollments PRIMARY KEY (StudentId, CourseId, Semester),
    CONSTRAINT FK_Enrollments_Students FOREIGN KEY (StudentId) REFERENCES Students(Id), 
    CONSTRAINT FK_Enrollments_Courses FOREIGN KEY (CourseId) REFERENCES Courses(Id),
);
GO