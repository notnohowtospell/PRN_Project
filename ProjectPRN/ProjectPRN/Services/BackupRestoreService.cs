using System.Text.Json;
using System.IO;
using BusinessObjects.Models;
using DataAccessObjects;
using ProjectPRN.DTOs;
using Microsoft.Win32;
using System.Windows;

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
        private readonly IStudentDAO _studentDAO;
        private readonly IInstructorDAO _instructorDAO;
        // Add other DAOs as needed

        public BackupRestoreService()
        {
            _studentDAO = new StudentDAO();
            _instructorDAO = new InstructorDAO();
            // Initialize other DAOs
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
            var backupData = new BackupData
            {
                BackupDate = DateTime.Now,
                Version = "1.0"
            };

            // Backup Students (without passwords)
            var students = await _studentDAO.GetAllAsync();
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
            var instructors = await _instructorDAO.GetAllAsync();
            backupData.Instructors = instructors.Select(i => new InstructorBackupDto
            {
                InstructorId = i.InstructorId,
                InstructorName = i.InstructorName,
                Experience = i.Experience,
                Email = i.Email,
                PhoneNumber = i.PhoneNumber,
                LastLogin = i.LastLogin
            }).ToList();

            // Add backup for other entities (Courses, Enrollments, etc.)
            // TODO: Implement backup for other entities

            return backupData;
        }

        public async Task<bool> RestoreFromBackupDataAsync(BackupData backupData)
        {
            try
            {
                // Show confirmation dialog
                var result = MessageBox.Show(
                    $"This will restore data from backup created on {backupData.BackupDate:yyyy-MM-dd HH:mm:ss}.\n" +
                    $"Students: {backupData.Students.Count}\n" +
                    $"Instructors: {backupData.Instructors.Count}\n\n" +
                    "This operation cannot be undone. Continue?",
                    "Confirm Restore",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return false;

                // Note: In a real application, you might want to:
                // 1. Create a backup of current data before restore
                // 2. Use transactions to ensure data consistency
                // 3. Handle ID conflicts (new vs existing records)
                
                // For now, we'll show a warning about the complexity
                MessageBox.Show(
                    "Note: Full restore functionality requires careful handling of:\n" +
                    "- Primary key conflicts\n" +
                    "- Foreign key relationships\n" +
                    "- Password reset for restored accounts\n\n" +
                    "Implementation should include proper transaction handling.",
                    "Restore Implementation Note",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Restore failed: {ex.Message}", "Restore Error", 
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