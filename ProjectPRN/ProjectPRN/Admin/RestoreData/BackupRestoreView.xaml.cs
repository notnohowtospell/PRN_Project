using System.IO;
using System.Text.Json;
using System.Windows;
using ProjectPRN.Services;
using DataAccessObjects;
using Microsoft.Win32;
using ProjectPRN.DTOs;

namespace ProjectPRN.Admin.BackupRestore
{
    public partial class BackupRestoreView : Window
    {
        private readonly IBackupRestoreService _backupRestoreService;
        private readonly IStudentDAO _studentDAO;
        private readonly IInstructorDAO _instructorDAO;

        public BackupRestoreView()
        {
            try
            {
                InitializeComponent();
                
                _backupRestoreService = new BackupRestoreService();
                _studentDAO = new StudentDAO();
                _instructorDAO = new InstructorDAO();
                
                Loaded += BackupRestoreView_Loaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khoi tao giao dien: {ex.Message}\n\nChi tiet: {ex.StackTrace}", "Loi Initialize", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private async void BackupRestoreView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi load du lieu: {ex.Message}", "Loi Load", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Statistics
        private async Task LoadStatisticsAsync()
        {
            try
            {
                UpdateStatus("Dang tai thong ke du lieu...");

                // Load statistics
                var students = await _studentDAO.GetAllAsync();
                var instructors = await _instructorDAO.GetAllAsync();
                
                // Update UI with statistics
                txtStudentCount.Text = students.Count().ToString();
                txtInstructorCount.Text = instructors.Count().ToString();
                txtCourseCount.Text = "0"; // TODO: Update when course DAO is available
                txtEnrollmentCount.Text = "0"; // TODO: Update when enrollment DAO is available
                txtLastUpdate.Text = $"Cap nhat: {DateTime.Now:HH:mm:ss dd/MM/yyyy}";
                
                UpdateStatus("Da tai thong ke du lieu thanh cong");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Loi khi tai thong ke: {ex.Message}");
                MessageBox.Show($"Khong the tai thong ke du lieu: {ex.Message}", "Loi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatus(string message)
        {
            try
            {
                if (txtStatus != null)
                    txtStatus.Text = message;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Loi cap nhat status: {ex.Message}");
            }
        }
        #endregion

        #region Backup Operations
        private async void BtnCreateBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus("Dang tao ban sao luu...");

                // Disable buttons during operation
                btnCreateBackup.IsEnabled = false;
                btnRestoreData.IsEnabled = false;

                // Get file path for backup
                var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"LifeSkillBackup_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                    Title = "Chon vi tri luu file sao luu"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var success = await _backupRestoreService.BackupDataToJsonAsync(saveDialog.FileName);
                    
                    if (success)
                    {
                        UpdateStatus($"Sao luu thanh cong: {Path.GetFileName(saveDialog.FileName)}");
                        
                        var result = MessageBox.Show(
                            $"Sao luu du lieu thanh cong!\n\n" +
                            $"File duoc luu tai:\n{saveDialog.FileName}\n\n" +
                            "Ban co muon mo thu muc chua file khong?",
                            "Sao luu thanh cong",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{saveDialog.FileName}\"");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Khong the mo thu muc: {ex.Message}", "Canh bao",
                                               MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }

                        await LoadStatisticsAsync();
                    }
                    else
                    {
                        UpdateStatus("Sao luu that bai");
                    }
                }
                else
                {
                    UpdateStatus("Da huy thao tac sao luu");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Loi khi sao luu: {ex.Message}");
                MessageBox.Show($"Loi khi tao ban sao luu: {ex.Message}", "Loi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable buttons
                btnCreateBackup.IsEnabled = true;
                btnRestoreData.IsEnabled = true;
            }
        }
        #endregion

        #region Restore Operations
        private async void BtnRestoreData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus("Dang chon file phuc hoi...");

                // Get file path for restore
                var openDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json",
                    Title = "Chon file sao luu de phuc hoi",
                    CheckFileExists = true
                };

                if (openDialog.ShowDialog() == true)
                {
                    // Read and validate backup file
                    UpdateStatus("Dang doc file sao luu...");

                    try
                    {
                        if (!File.Exists(openDialog.FileName))
                        {
                            MessageBox.Show("File khong ton tai.", "Loi file",
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                            UpdateStatus("File khong ton tai");
                            return;
                        }

                        var json = await File.ReadAllTextAsync(openDialog.FileName);
                        
                        if (string.IsNullOrWhiteSpace(json))
                        {
                            MessageBox.Show("File rong hoac khong co du lieu.", "Loi file sao luu",
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                            UpdateStatus("File rong");
                            return;
                        }

                        var backupData = JsonSerializer.Deserialize<BackupData>(json, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                        if (backupData == null)
                        {
                            MessageBox.Show("File sao luu khong hop le hoac bi hong.", "Loi file sao luu",
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                            UpdateStatus("File sao luu khong hop le");
                            return;
                        }

                        // Validate backup data structure
                        if (backupData.Students == null || backupData.Instructors == null || 
                            backupData.Courses == null || backupData.Enrollments == null)
                        {
                            MessageBox.Show("Cau truc file sao luu khong day du.", "Loi file sao luu",
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                            UpdateStatus("Cau truc file khong hop le");
                            return;
                        }

                        UpdateStatus("File sao luu hop le, hien thi thong tin xac nhan...");

                        // Simple confirmation dialog instead of Material Design dialog
                        var confirmResult = MessageBox.Show(
                            $"Ban co chac chan muon phuc hoi du lieu tu file backup nay khong?\n\n" +
                            $"Thong tin file:\n" +
                            $"- Ngay tao: {backupData.BackupDate:dd/MM/yyyy HH:mm:ss}\n" +
                            $"- Phien ban: {backupData.Version}\n" +
                            $"- Hoc vien: {backupData.Students?.Count ?? 0}\n" +
                            $"- Giang vien: {backupData.Instructors?.Count ?? 0}\n" +
                            $"- Khoa hoc: {backupData.Courses?.Count ?? 0}\n\n" +
                            "CANH BAO: Thao tac nay se ghi de toan bo du lieu hien tai!\n" +
                            "Ban co chac chan muon tiep tuc khong?",
                            "Xac nhan phuc hoi du lieu",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (confirmResult == MessageBoxResult.Yes)
                        {
                            // Proceed with restore
                            UpdateStatus("Dang phuc hoi du lieu...");

                            // Disable buttons during operation
                            btnCreateBackup.IsEnabled = false;
                            btnRestoreData.IsEnabled = false;

                            var success = await _backupRestoreService.RestoreDataFromJsonAsync(openDialog.FileName);
                            
                            if (success)
                            {
                                UpdateStatus("Phuc hoi du lieu thanh cong");
                                
                                MessageBox.Show(
                                    "Phuc hoi du lieu thanh cong!\n\n" +
                                    "Luu y quan trong:\n" +
                                    "• Tat ca mat khau can duoc reset\n" +
                                    "• Vui long khoi dong lai ung dung\n" +
                                    "• Kiem tra tinh toan ven cua du lieu\n\n" +
                                    "He thong se tu dong lam moi thong ke.",
                                    "Phuc hoi thanh cong",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                // Refresh statistics after restore
                                await LoadStatisticsAsync();
                            }
                            else
                            {
                                UpdateStatus("Phuc hoi du lieu that bai");
                            }
                        }
                        else
                        {
                            UpdateStatus("Da huy thao tac phuc hoi");
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        MessageBox.Show($"File khong phai la dinh dang JSON hop le.\n\nLoi chi tiet: {jsonEx.Message}", 
                                       "Loi dinh dang file",
                                       MessageBoxButton.OK, MessageBoxImage.Error);
                        UpdateStatus("File khong dung dinh dang JSON");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Khong co quyen truy cap file. Vui long kiem tra quyen doc file.", 
                                       "Loi quyen truy cap",
                                       MessageBoxButton.OK, MessageBoxImage.Error);
                        UpdateStatus("Khong co quyen truy cap file");
                    }
                    catch (IOException ioEx)
                    {
                        MessageBox.Show($"Loi khi doc file.\n\nLoi chi tiet: {ioEx.Message}", 
                                       "Loi doc file",
                                       MessageBoxButton.OK, MessageBoxImage.Error);
                        UpdateStatus("Loi doc file");
                    }
                }
                else
                {
                    UpdateStatus("Da huy chon file phuc hoi");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Loi khi phuc hoi: {ex.Message}");
                MessageBox.Show($"Loi khi phuc hoi du lieu: {ex.Message}", "Loi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable buttons
                btnCreateBackup.IsEnabled = true;
                btnRestoreData.IsEnabled = true;
            }
        }
        #endregion

        #region Event Handlers
        private async void BtnRefreshStats_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi refresh: {ex.Message}", "Loi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                base.OnClosed(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Loi khi dong window: {ex.Message}");
            }
        }
        #endregion
    }
}