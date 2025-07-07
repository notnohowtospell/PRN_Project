USE master
GO
-- Xóa cơ sở dữ liệu nếu đã tồn tại
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'PRN212SkillsHoannn6')
BEGIN
    ALTER DATABASE PRN212SkillsHoannn6 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PRN212SkillsHoannn6;
END
GO
-- Tạo cơ sở dữ liệu mới
CREATE DATABASE PRN212SkillsHoannn6
GO
USE [PRN212SkillsHoannn6]
GO

-- Tạo bảng __EFMigrationsHistory
CREATE TABLE [dbo].[__EFMigrationsHistory](
    [MigrationId] [nvarchar](150) NOT NULL,
    [ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
    [MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Tạo bảng Students
CREATE TABLE [dbo].[Students](
    [StudentId] [int] IDENTITY(1,1) NOT NULL,
    [StudentCode] [nvarchar](max) NOT NULL,
    [StudentName] [nvarchar](max) NOT NULL,
    [Password] [nvarchar](max) NOT NULL,
    [Email] [nvarchar](max) NULL,
    [Status] [nvarchar](50) NULL,
    [PhoneNumber] [nvarchar](20) NULL,
    [DateOfBirth] [datetime2](7) NULL,
    [AvatarPath] [nvarchar](max) NULL,
    [LastLogin] [datetime2](7) NULL,
 CONSTRAINT [PK_Students] PRIMARY KEY CLUSTERED 
(
    [StudentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Tạo bảng Instructors
CREATE TABLE [dbo].[Instructors](
    [InstructorId] [int] IDENTITY(1,1) NOT NULL,
    [InstructorName] [nvarchar](max) NOT NULL,
    [Experience] [int] NOT NULL,
    [Email] [nvarchar](max) NULL,
    [PhoneNumber] [nvarchar](20) NULL,
 CONSTRAINT [PK_Instructors] PRIMARY KEY CLUSTERED 
(
    [InstructorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Tạo bảng LifeSkillCourses
CREATE TABLE [dbo].[LifeSkillCourses](
    [CourseId] [int] IDENTITY(1,1) NOT NULL,
    [CourseName] [nvarchar](max) NOT NULL,
    [InstructorId] [int] NOT NULL,
    [StartDate] [datetime2](7) NULL,
    [EndDate] [datetime2](7) NULL,
    [Description] [nvarchar](max) NULL,
    [MaxStudents] [int] NULL,
    [Price] [decimal](18,2) NULL,
    [Status] [nvarchar](50) NULL,
 CONSTRAINT [PK_LifeSkillCourses] PRIMARY KEY CLUSTERED 
(
    [CourseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Tạo bảng Enrollments
CREATE TABLE [dbo].[Enrollments](
    [EnrollmentId] [int] IDENTITY(1,1) NOT NULL,
    [StudentId] [int] NOT NULL,
    [CourseId] [int] NOT NULL,
    [CompletionStatus] [bit] NOT NULL,
    [CompletionDate] [datetime2](7) NULL,
 CONSTRAINT [PK_Enrollments] PRIMARY KEY CLUSTERED 
(
    [EnrollmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Tạo bảng Assessments
CREATE TABLE [dbo].[Assessments](
    [AssessmentId] [int] IDENTITY(1,1) NOT NULL,
    [CourseId] [int] NOT NULL,
    [AssessmentName] [nvarchar](max) NOT NULL,
    [MaxScore] [int] NOT NULL,
    [DueDate] [datetime2](7) NOT NULL,
    [AssessmentType] [nvarchar](50) NULL,
    [Instructions] [nvarchar](max) NULL,
 CONSTRAINT [PK_Assessments] PRIMARY KEY CLUSTERED 
(
    [AssessmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Tạo bảng AssessmentResults
CREATE TABLE [dbo].[AssessmentResults](
    [ResultId] [int] IDENTITY(1,1) NOT NULL,
    [AssessmentId] [int] NOT NULL,
    [StudentId] [int] NOT NULL,
    [Score] [decimal](4,1) NULL,
    [SubmissionDate] [datetime2](7) NULL,
 CONSTRAINT [PK_AssessmentResults] PRIMARY KEY CLUSTERED 
(
    [ResultId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Tạo bảng Notifications
CREATE TABLE [dbo].[Notifications](
    [NotificationId] [int] IDENTITY(1,1) NOT NULL,
    [Title] [nvarchar](max) NOT NULL,
    [Content] [nvarchar](max) NOT NULL,
    [StudentId] [int] NULL,
    [CreatedDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED 
(
    [NotificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Tạo bảng CourseMaterials
CREATE TABLE [dbo].[CourseMaterials](
    [MaterialId] [int] IDENTITY(1,1) NOT NULL,
    [CourseId] [int] NOT NULL,
    [Title] [nvarchar](max) NOT NULL,
    [FilePath] [nvarchar](max) NOT NULL,
    [UploadDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_CourseMaterials] PRIMARY KEY CLUSTERED 
(
    [MaterialId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Tạo bảng Feedbacks
CREATE TABLE [dbo].[Feedbacks](
    [FeedbackId] [int] IDENTITY(1,1) NOT NULL,
    [StudentId] [int] NOT NULL,
    [CourseId] [int] NOT NULL,
    [Rating] [int] NOT NULL,
    [Comment] [nvarchar](max) NULL,
    [FeedbackDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Feedbacks] PRIMARY KEY CLUSTERED 
(
    [FeedbackId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Tạo bảng Certificates
CREATE TABLE [dbo].[Certificates](
    [CertificateId] [int] IDENTITY(1,1) NOT NULL,
    [StudentId] [int] NOT NULL,
    [CourseId] [int] NOT NULL,
    [IssueDate] [datetime2](7) NOT NULL,
    [CertificateCode] [nvarchar](50) NOT NULL,
    [FilePath] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Certificates] PRIMARY KEY CLUSTERED 
(
    [CertificateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UK_Certificates_CertificateCode] UNIQUE NONCLUSTERED 
(
    [CertificateCode] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Tạo bảng CourseSchedules
CREATE TABLE [dbo].[CourseSchedules](
    [ScheduleId] [int] IDENTITY(1,1) NOT NULL,
    [CourseId] [int] NOT NULL,
    [SessionDate] [datetime2](7) NOT NULL,
    [StartTime] [time](7) NOT NULL,
    [EndTime] [time](7) NOT NULL,
    [Room] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CourseSchedules] PRIMARY KEY CLUSTERED 
(
    [ScheduleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Tạo bảng ActivityLogs
CREATE TABLE [dbo].[ActivityLogs](
    [LogId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NULL,
    [Action] [nvarchar](max) NOT NULL,
    [Timestamp] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_ActivityLogs] PRIMARY KEY CLUSTERED 
(
    [LogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Tạo bảng Payments
CREATE TABLE [dbo].[Payments](
    [PaymentId] [int] IDENTITY(1,1) NOT NULL,
    [StudentId] [int] NOT NULL,
    [CourseId] [int] NOT NULL,
    [Amount] [decimal](18,2) NOT NULL,
    [PaymentDate] [datetime2](7) NOT NULL,
    [Status] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Payments] PRIMARY KEY CLUSTERED 
(
    [PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Thêm ràng buộc khóa ngoại
ALTER TABLE [dbo].[Enrollments]  WITH CHECK ADD  CONSTRAINT [FK_Enrollments_LifeSkillCourses_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[LifeSkillCourses] ([CourseId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Enrollments] CHECK CONSTRAINT [FK_Enrollments_LifeSkillCourses_CourseId]
GO
ALTER TABLE [dbo].[Enrollments]  WITH CHECK ADD  CONSTRAINT [FK_Enrollments_Students_StudentId] FOREIGN KEY([StudentId])
REFERENCES [dbo].[Students] ([StudentId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Enrollments] CHECK CONSTRAINT [FK_Enrollments_Students_StudentId]
GO

ALTER TABLE [dbo].[LifeSkillCourses]  WITH CHECK ADD  CONSTRAINT [FK_LifeSkillCourses_Instructors_InstructorId] FOREIGN KEY([InstructorId])
REFERENCES [dbo].[Instructors] ([InstructorId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LifeSkillCourses] CHECK CONSTRAINT [FK_LifeSkillCourses_Instructors_InstructorId]
GO

ALTER TABLE [dbo].[Assessments]  WITH CHECK ADD  CONSTRAINT [FK_Assessments_LifeSkillCourses_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[LifeSkillCourses] ([CourseId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Assessments] CHECK CONSTRAINT [FK_Assessments_LifeSkillCourses_CourseId]
GO

ALTER TABLE [dbo].[AssessmentResults]  WITH CHECK ADD  CONSTRAINT [FK_AssessmentResults_Assessments_AssessmentId] FOREIGN KEY([AssessmentId])
REFERENCES [dbo].[Assessments] ([AssessmentId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AssessmentResults] CHECK CONSTRAINT [FK_AssessmentResults_Assessments_AssessmentId]
GO
ALTER TABLE [dbo].[AssessmentResults]  WITH CHECK ADD  CONSTRAINT [FK_AssessmentResults_Students_StudentId] FOREIGN KEY([StudentId])
REFERENCES [dbo].[Students] ([StudentId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AssessmentResults] CHECK CONSTRAINT [FK_AssessmentResults_Students_StudentId]
GO

ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_Students_StudentId] FOREIGN KEY([StudentId])
REFERENCES [dbo].[Students] ([StudentId])
GO
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Students_StudentId]
GO

ALTER TABLE [dbo].[CourseMaterials]  WITH CHECK ADD  CONSTRAINT [FK_CourseMaterials_LifeSkillCourses_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[LifeSkillCourses] ([CourseId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CourseMaterials] CHECK CONSTRAINT [FK_CourseMaterials_LifeSkillCourses_CourseId]
GO

ALTER TABLE [dbo].[Feedbacks]  WITH CHECK ADD  CONSTRAINT [FK_Feedbacks_Students_StudentId] FOREIGN KEY([StudentId])
REFERENCES [dbo].[Students] ([StudentId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Feedbacks] CHECK CONSTRAINT [FK_Feedbacks_Students_StudentId]
GO
ALTER TABLE [dbo].[Feedbacks]  WITH CHECK ADD  CONSTRAINT [FK_Feedbacks_LifeSkillCourses_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[LifeSkillCourses] ([CourseId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Feedbacks] CHECK CONSTRAINT [FK_Feedbacks_LifeSkillCourses_CourseId]
GO

ALTER TABLE [dbo].[Certificates]  WITH CHECK ADD  CONSTRAINT [FK_Certificates_Students_StudentId] FOREIGN KEY([StudentId])
REFERENCES [dbo].[Students] ([StudentId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Certificates] CHECK CONSTRAINT [FK_Certificates_Students_StudentId]
GO
ALTER TABLE [dbo].[Certificates]  WITH CHECK ADD  CONSTRAINT [FK_Certificates_LifeSkillCourses_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[LifeSkillCourses] ([CourseId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Certificates] CHECK CONSTRAINT [FK_Certificates_LifeSkillCourses_CourseId]
GO

ALTER TABLE [dbo].[CourseSchedules]  WITH CHECK ADD  CONSTRAINT [FK_CourseSchedules_LifeSkillCourses_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[LifeSkillCourses] ([CourseId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CourseSchedules] CHECK CONSTRAINT [FK_CourseSchedules_LifeSkillCourses_CourseId]
GO

ALTER TABLE [dbo].[ActivityLogs]  WITH CHECK ADD  CONSTRAINT [FK_ActivityLogs_Students_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Students] ([StudentId])
GO
ALTER TABLE [dbo].[ActivityLogs] CHECK CONSTRAINT [FK_ActivityLogs_Students_UserId]
GO

ALTER TABLE [dbo].[Payments]  WITH CHECK ADD  CONSTRAINT [FK_Payments_Students_StudentId] FOREIGN KEY([StudentId])
REFERENCES [dbo].[Students] ([StudentId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payments_Students_StudentId]
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD  CONSTRAINT [FK_Payments_LifeSkillCourses_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[LifeSkillCourses] ([CourseId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payments_LifeSkillCourses_CourseId]
GO

-- Chèn dữ liệu mẫu
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) 
VALUES (N'20250312141859_InitialCreate', N'8.0.14')
GO

SET IDENTITY_INSERT [dbo].[Students] ON 
INSERT [dbo].[Students] ([StudentId], [StudentCode], [StudentName], [Password], [Email], [Status], [PhoneNumber], [DateOfBirth], [AvatarPath], [LastLogin]) 
VALUES (1, N'SE131517', N'Nguyễn Văn An', N'$2a$11$3X8G9f6z3Q7k0vL2nN5u5O5pQ4mW8k9j0rH1tY2u3vW4xZ5y6A7B', N'an.nguyen@example.com', N'Hoạt động', N'0123456789', N'2003-01-01', N'AppData/Avatars/SE131517.jpg', N'2025-06-19 08:00:00')
SET IDENTITY_INSERT [dbo].[Students] OFF
GO

SET IDENTITY_INSERT [dbo].[Instructors] ON 
INSERT [dbo].[Instructors] ([InstructorId], [InstructorName], [Experience], [Email], [PhoneNumber]) 
VALUES (1, N'MS. Hoai', 5, N'hoai@example.com', N'0987654321'),
       (2, N'MR. Quang', 5, N'quang@example.com', N'0987654322')
SET IDENTITY_INSERT [dbo].[Instructors] OFF
GO

SET IDENTITY_INSERT [dbo].[LifeSkillCourses] ON 
INSERT [dbo].[LifeSkillCourses] ([CourseId], [CourseName], [InstructorId], [StartDate], [EndDate], [Description], [MaxStudents], [Price], [Status]) 
VALUES (1, N'.NET', 2, N'2025-06-01', N'2025-08-01', N'Khóa học lập trình .NET cơ bản', 30, 1500000.00, N'Mở đăng ký'),
       (2, N'Java', 1, N'2025-06-01', N'2025-08-01', N'Khóa học lập trình Java nâng cao', 25, 2000000.00, N'Mở đăng ký')
SET IDENTITY_INSERT [dbo].[LifeSkillCourses] OFF
GO

SET IDENTITY_INSERT [dbo].[Enrollments] ON 
INSERT [dbo].[Enrollments] ([EnrollmentId], [StudentId], [CourseId], [CompletionStatus], [CompletionDate]) 
VALUES (1, 1, 2, 0, NULL)
SET IDENTITY_INSERT [dbo].[Enrollments] OFF
GO

SET IDENTITY_INSERT [dbo].[Assessments] ON 
INSERT [dbo].[Assessments] ([AssessmentId], [CourseId], [AssessmentName], [MaxScore], [DueDate], [AssessmentType], [Instructions]) 
VALUES (1, 2, N'Kiểm tra cuối kỳ Java', 100, N'2025-07-30', N'Trắc nghiệm', N'Hoàn thành bài kiểm tra trong 60 phút.')
SET IDENTITY_INSERT [dbo].[Assessments] OFF
GO

SET IDENTITY_INSERT [dbo].[AssessmentResults] ON 
INSERT [dbo].[AssessmentResults] ([ResultId], [AssessmentId], [StudentId], [Score], [SubmissionDate]) 
VALUES (1, 1, 1, 85.5, N'2025-07-29')
SET IDENTITY_INSERT [dbo].[AssessmentResults] OFF
GO

SET IDENTITY_INSERT [dbo].[Notifications] ON 
INSERT [dbo].[Notifications] ([NotificationId], [Title], [Content], [StudentId], [CreatedDate]) 
VALUES (1, N'Nhắc nhở nộp bài', N'Vui lòng nộp bài kiểm tra Java trước 30/07/2025', 1, N'2025-07-25')
SET IDENTITY_INSERT [dbo].[Notifications] OFF
GO

SET IDENTITY_INSERT [dbo].[CourseMaterials] ON 
INSERT [dbo].[CourseMaterials] ([MaterialId], [CourseId], [Title], [FilePath], [UploadDate]) 
VALUES (1, 2, N'Tài liệu Java cơ bản', N'AppData/Materials/Java_Basic.pdf', N'2025-06-01')
SET IDENTITY_INSERT [dbo].[CourseMaterials] OFF
GO

SET IDENTITY_INSERT [dbo].[Feedbacks] ON 
INSERT [dbo].[Feedbacks] ([FeedbackId], [StudentId], [CourseId], [Rating], [Comment], [FeedbackDate]) 
VALUES (1, 1, 2, 4, N'Khóa học rất hữu ích nhưng cần thêm bài tập thực hành.', N'2025-07-31')
SET IDENTITY_INSERT [dbo].[Feedbacks] OFF
GO

SET IDENTITY_INSERT [dbo].[Certificates] ON 
INSERT [dbo].[Certificates] ([CertificateId], [StudentId], [CourseId], [IssueDate], [CertificateCode], [FilePath]) 
VALUES (1, 1, 2, N'2025-08-01', N'CERT-001', N'AppData/Certificates/CERT-001.pdf')
SET IDENTITY_INSERT [dbo].[Certificates] OFF
GO

SET IDENTITY_INSERT [dbo].[CourseSchedules] ON 
INSERT [dbo].[CourseSchedules] ([ScheduleId], [CourseId], [SessionDate], [StartTime], [EndTime], [Room]) 
VALUES (1, 2, N'2025-06-10', N'08:00:00', N'10:00:00', N'P101')
SET IDENTITY_INSERT [dbo].[CourseSchedules] OFF
GO

SET IDENTITY_INSERT [dbo].[ActivityLogs] ON 
INSERT [dbo].[ActivityLogs] ([LogId], [UserId], [Action], [Timestamp]) 
VALUES (1, 1, N'Đăng nhập thành công: SE131517', N'2025-06-19 08:00:00')
SET IDENTITY_INSERT [dbo].[ActivityLogs] OFF
GO

SET IDENTITY_INSERT [dbo].[Payments] ON 
INSERT [dbo].[Payments] ([PaymentId], [StudentId], [CourseId], [Amount], [PaymentDate], [Status]) 
VALUES (1, 1, 2, 2000000.00, N'2025-06-01', N'Đã thanh toán')
SET IDENTITY_INSERT [dbo].[Payments] OFF
GO