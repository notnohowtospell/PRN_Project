using System.Windows;
using System.Windows.Controls;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;

namespace ProjectPRN.Admin.InstructorManagement
{
    public partial class InstructorDetailsDialog : UserControl
    {
        private readonly InstructorViewModel _instructor;
        private readonly ApplicationDbContext _context;

        public InstructorDetailsDialog(InstructorViewModel instructor)
        {
            InitializeComponent();

            _instructor = instructor;
            _context = new ApplicationDbContext();

            LoadInstructorDetails();
        }

        #region Initialization
        private void LoadInstructorDetails()
        {
            // Basic information
            txtInstructorName.Text = _instructor.InstructorName;
            txtEmail.Text = _instructor.Email ?? "Không có thông tin";
            txtPhoneNumber.Text = _instructor.PhoneNumber ?? "Không có thông tin";
            txtExperience.Text = $"{_instructor.Experience} năm";
            txtInstructorId.Text = _instructor.InstructorId.ToString();

            // Last login
            txtLastLogin.Text = _instructor.LastLogin?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa đăng nhập";

            // Load course statistics
            LoadCourseStatistics();
        }

        private void LoadCourseStatistics()
        {
            try
            {
                var instructor = _context.Instructors
                    .Include(i => i.LifeSkillCourses)
                    .FirstOrDefault(i => i.InstructorId == _instructor.InstructorId);

                if (instructor != null)
                {
                    var totalCourses = instructor.LifeSkillCourses.Count;
                    var activeCourses = instructor.LifeSkillCourses.Count(c => c.Status == "Mở đăng ký");
                    var completedCourses = instructor.LifeSkillCourses.Count(c => c.Status == "Đã đóng");

                    txtTotalCourses.Text = totalCourses.ToString();
                    txtActiveCourses.Text = activeCourses.ToString();
                    txtCompletedCourses.Text = completedCourses.ToString();
                }
                else
                {
                    txtTotalCourses.Text = "0";
                    txtActiveCourses.Text = "0";
                    txtCompletedCourses.Text = "0";
                }
            }
            catch (Exception ex)
            {
                // If error loading statistics, show default values
                txtTotalCourses.Text = _instructor.CourseCount.ToString();
                txtActiveCourses.Text = "N/A";
                txtCompletedCourses.Text = "N/A";
            }
        }
        #endregion

        #region Event Handlers
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Close this dialog and return the instructor for editing
            var result = new InstructorDetailsResult
            {
                ShouldEdit = true,
                Instructor = _instructor
            };

            DialogHost.CloseDialogCommand.Execute(result, this);
        }
        #endregion
    }

    // Result class for dialog communication
    public class InstructorDetailsResult
    {
        public bool ShouldEdit { get; set; }
        public InstructorViewModel Instructor { get; set; }
    }
}