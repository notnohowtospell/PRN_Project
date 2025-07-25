using System.Text.Json;
using System.IO;
using BusinessObjects.Models;
using ProjectPRN.DTOs;
using Microsoft.Win32;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace ProjectPRN.Services
{
    public interface IBackupRestoreService
    {
        Task<bool> BackupDataToJsonAsync(string filePath);
        Task<bool> RestoreDataFromJsonAsync(string filePath);
        Task<BackupData> CreateBackupDataAsync();
        Task<bool> RestoreFromBackupDataAsync(BackupData backupData);
    }

    public class BackupRestoreService : IBackupRestoreService
    {
        public BackupRestoreService()
        {
            // No need to initialize DAOs, we'll use DbContext directly
        }

        public async Task<bool> BackupDataToJsonAsync(string filePath)
        {
            try
            {
                var backupData = await CreateBackupDataAsync();
                
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(backupData, options);
                await File.WriteAllTextAsync(filePath, json);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed: {ex.Message}", "Backup Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RestoreDataFromJsonAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Backup file not found.", "Restore Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                var json = await File.ReadAllTextAsync(filePath);
                var backupData = JsonSerializer.Deserialize<BackupData>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (backupData == null)
                {
                    MessageBox.Show("Invalid backup file format.", "Restore Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                return await RestoreFromBackupDataAsync(backupData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Restore failed: {ex.Message}", "Restore Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<BackupData> CreateBackupDataAsync()
        {
            using var context = new ApplicationDbContext();
            
            var backupData = new BackupData
            {
                BackupDate = DateTime.Now,
                Version = "1.0"
            };

            try
            {
                // Backup Students (without passwords)
                var students = await context.Students
                    .Include(s => s.Enrollments)
                    .Include(s => s.Certificates)
                    .Include(s => s.AssessmentResults)
                    .ToListAsync();

                backupData.Students = students.Select(s => new StudentBackupDto
                {
                    StudentId = s.StudentId,
                    StudentCode = s.StudentCode,
                    StudentName = s.StudentName,
                    Email = s.Email,
                    Status = s.Status,
                    PhoneNumber = s.PhoneNumber,
                    DateOfBirth = s.DateOfBirth,
                    AvatarPath = s.AvatarPath,
                    LastLogin = s.LastLogin
                }).ToList();

                // Backup Instructors (without passwords)
                var instructors = await context.Instructors
                    .Include(i => i.LifeSkillCourses)
                    .ToListAsync();

                backupData.Instructors = instructors.Select(i => new InstructorBackupDto
                {
                    InstructorId = i.InstructorId,
                    InstructorName = i.InstructorName,
                    Experience = i.Experience,
                    Email = i.Email,
                    PhoneNumber = i.PhoneNumber,
                    LastLogin = i.LastLogin
                }).ToList();

                // Backup Courses
                var courses = await context.LifeSkillCourses
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .Include(c => c.Assessments)
                    .ToListAsync();

                backupData.Courses = courses.Select(c => new CourseBackupDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    Price = c.Price,
                    Status = c.Status,
                    InstructorId = c.InstructorId
                }).ToList();

                return backupData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating backup data: {ex.Message}", "Backup Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task<bool> RestoreFromBackupDataAsync(BackupData backupData)
        {
            using var context = new ApplicationDbContext();

            try
            {
                // Clear existing data
                context.Students.RemoveRange(context.Students);
                context.Instructors.RemoveRange(context.Instructors);
                context.LifeSkillCourses.RemoveRange(context.LifeSkillCourses);
                await context.SaveChangesAsync();

                // Restore Students
                var students = backupData.Students.Select(s => new BusinessObjects.Models.Student
                {
                    StudentId = s.StudentId,
                    StudentCode = s.StudentCode,
                    StudentName = s.StudentName,
                    Email = s.Email,
                    Status = s.Status,
                    PhoneNumber = s.PhoneNumber,
                    DateOfBirth = s.DateOfBirth,
                    AvatarPath = s.AvatarPath,
                    LastLogin = s.LastLogin
                }).ToList();

                await context.Students.AddRangeAsync(students);

                // Restore Instructors
                var instructors = backupData.Instructors.Select(i => new Instructor
                {
                    InstructorId = i.InstructorId,
                    InstructorName = i.InstructorName,
                    Experience = i.Experience,
                    Email = i.Email,
                    PhoneNumber = i.PhoneNumber,
                    LastLogin = i.LastLogin
                }).ToList();

                await context.Instructors.AddRangeAsync(instructors);

                // Restore Courses
                foreach (var courseDto in backupData.Courses)
                {
                    // Find the instructor by name (since IDs might have changed)
                    var instructorName = backupData.Instructors.FirstOrDefault(inst => inst.InstructorId == courseDto.InstructorId)?.InstructorName;
                    var instructor = await context.Instructors
                        .FirstOrDefaultAsync(i => i.InstructorName == instructorName);

                    var course = new LifeSkillCourse
                    {
                        // Don't set CourseId, let it auto-increment
                        CourseName = courseDto.CourseName,
                        InstructorId = instructor?.InstructorId ?? 1, // Default to first instructor if not found
                        Description = courseDto.Description,
                        Price = courseDto.Price,
                        Status = courseDto.Status ?? "?ang m?"
                    };
                    context.LifeSkillCourses.Add(course);
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring data: {ex.Message}", "Restore Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Helper method to open file dialog for backup
        public string? GetBackupFilePath(bool save = true)
        {
            if (save)
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"LifeSkillBackup_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };
                return saveDialog.ShowDialog() == true ? saveDialog.FileName : null;
            }
            else
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json"
                };
                return openDialog.ShowDialog() == true ? openDialog.FileName : null;
            }
        }
    }
}