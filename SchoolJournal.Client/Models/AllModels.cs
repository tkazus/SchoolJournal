using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public class StudentDisplayItem
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public double AverageGrade { get; set; }
        public double Attendance { get; set; }
        public int Tokens { get; set; }
    }

    public class SubjectDisplayItem
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
    }

    public class GradeDisplayItem
    {
        public string Subject { get; set; }
        public int Value { get; set; }
        public DateTime Date { get; set; }
        public string Teacher { get; set; }
        public Brush GradeColor { get; set; }
        public Brush GradeTextColor { get; set; }
    }

    public class GradeDisplayFullItem
    {
        public string Subject { get; set; }
        public int Value { get; set; }
        public DateTime Date { get; set; }
        public string Teacher { get; set; }
        public string Comment { get; set; }
        public Brush GradeColor { get; set; }
        public Brush GradeTextColor { get; set; }
    }

    public class ScheduleDisplayItem
    {
        public string Subject { get; set; }
        public string Teacher { get; set; }
        public string Time { get; set; }
        public string Room { get; set; }
    }

    public class ScheduleDisplayFullItem
    {
        public string DayName { get; set; }
        public string Subject { get; set; }
        public string Teacher { get; set; }
        public string Time { get; set; }
        public string Room { get; set; }
    }

    public class HomeworkDisplayItem
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class HomeworkDisplayFullItem
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Teacher { get; set; }
        public Brush DueDateColor { get; set; }
        public Brush DueDateTextColor { get; set; }
    }

    public class AttendanceDisplayItem
    {
        public string Subject { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
    }

    public class AttendanceDisplayFullItem
    {
        public string Subject { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public Brush StatusColor { get; set; }
        public Brush StatusTextColor { get; set; }
    }

    public class TokenTransactionDisplayItem
    {
        public int Amount { get; set; }
        public string AmountText { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
        public DateTime Date { get; set; }
        public int Balance { get; set; }
        public Brush AmountColor { get; set; }
        public Brush AmountTextColor { get; set; }
    }

    public class ShopItemDisplay
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
    }

    public class PurchaseDisplayItem
    {
        public string ItemName { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public int SpentTokens { get; set; }
    }

    public class UserDisplayItem
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string Class { get; set; }
        public string Specialty { get; set; }
    }

    public class ClassDisplayItem
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int YearOfStudy { get; set; }
        public int StudentCount { get; set; }
        public double AverageGrade { get; set; }
    }

    public class StudentStatItem
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public double AverageGrade { get; set; }
        public double Attendance { get; set; }
        public int Tokens { get; set; }
        public List<GradeStatItem> RecentGrades { get; set; }
        public Brush GradeColor { get; set; }
        public Brush GradeTextColor { get; set; }
    }

    public class GradeStatItem
    {
        public int Value { get; set; }
        public Brush GradeColor { get; set; }
        public Brush GradeTextColor { get; set; }
    }

    public class StudentAdminItem
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string ClassName { get; set; }
        public string BirthDate { get; set; }
        public string Login { get; set; }
    }

    public class TeacherAdminItem
    {
        public int TeacherId { get; set; }
        public string FullName { get; set; }
        public string Specialty { get; set; }
        public string Login { get; set; }
    }

    public class SubjectAdminItem
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
    }

    public class ScheduleAdminItem
    {
        public int ScheduleId { get; set; }
        public string ClassName { get; set; }
        public string SubjectName { get; set; }
        public string TeacherName { get; set; }
        public string DayName { get; set; }
        public string Time { get; set; }
    }
}