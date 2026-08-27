using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace SchoolJournal.Server
{
    public class Database
    {
        private string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=SchoolJournal;Integrated Security=True;TrustServerCertificate=True;";

        public string LoginUser(string login, string passwordHash)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT u.UserId, u.FullName, r.RoleName, u.IsActive
                    FROM Users u
                    JOIN Roles r ON u.RoleId = r.RoleId
                    WHERE u.Login = @Login AND u.PasswordHash = @PasswordHash", conn);
                cmd.Parameters.AddWithValue("@Login", login);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return $"{reader["UserId"]}|{reader["FullName"]}|{reader["RoleName"]}|{reader["IsActive"]}";
                    }
                }
            }
            return "ERROR|Неверный логин или пароль";
        }

        public string GetUserRole(int userId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT r.RoleName FROM Users u
                    JOIN Roles r ON u.RoleId = r.RoleId
                    WHERE u.UserId = @UserId", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                return cmd.ExecuteScalar()?.ToString() ?? "Unknown";
            }
        }


        public string GetStatistics(int studentId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Средний балл
                var avgCmd = new SqlCommand("SELECT ISNULL(AVG(CAST(GradeValue AS FLOAT)), 0) FROM Grades WHERE StudentId = @StudentId", conn);
                avgCmd.Parameters.AddWithValue("@StudentId", studentId);
                double avgGrade = Convert.ToDouble(avgCmd.ExecuteScalar());

                // Посещаемость
                var attCmd = new SqlCommand(@"SELECT ISNULL(CAST(SUM(CASE WHEN Status = N'Присутствовал' THEN 1 ELSE 0 END) AS FLOAT) / NULLIF(COUNT(*), 0) * 100, 0) FROM Attendance WHERE StudentId = @StudentId", conn);
                attCmd.Parameters.AddWithValue("@StudentId", studentId);
                double attendance = Convert.ToDouble(attCmd.ExecuteScalar());

                // ДЗ на сегодня
                var hwCmd = new SqlCommand(@"SELECT COUNT(*) FROM Homeworks h
                    JOIN TeacherClassSubject tcs ON h.ClassId = tcs.ClassId
                    JOIN Students s ON s.ClassId = tcs.ClassId
                    WHERE s.StudentId = @StudentId 
                    AND CAST(GETDATE() AS DATE) BETWEEN CAST(h.IssueDate AS DATE) AND CAST(h.DueDate AS DATE)", conn);
                hwCmd.Parameters.AddWithValue("@StudentId", studentId);
                int todayHw = Convert.ToInt32(hwCmd.ExecuteScalar());

                // Награды 
                var awardsCmd = new SqlCommand("SELECT COUNT(*) FROM Grades WHERE StudentId = @StudentId AND GradeValue = 5", conn);
                awardsCmd.Parameters.AddWithValue("@StudentId", studentId);
                int awards = Convert.ToInt32(awardsCmd.ExecuteScalar());

                // Баланс жетонов 
                var tokenCmd = new SqlCommand("SELECT ISNULL(TokenBalance, 0) FROM Students WHERE StudentId = @StudentId", conn);
                tokenCmd.Parameters.AddWithValue("@StudentId", studentId);
                int tokens = Convert.ToInt32(tokenCmd.ExecuteScalar());

                // Имя ученика
                var nameCmd = new SqlCommand(@"SELECT u.FullName FROM Users u
                    JOIN Students s ON s.UserId = u.UserId
                    WHERE s.StudentId = @StudentId", conn);
                nameCmd.Parameters.AddWithValue("@StudentId", studentId);
                string name = nameCmd.ExecuteScalar()?.ToString() ?? "Ученик";

                return $"{avgGrade:F1}|{attendance:F0}|{todayHw}|{awards}|{tokens}|{name}";
            }
        }


        public string GetRecentGrades(int studentId, int count = 5)
        {
            var grades = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT TOP (@Count) g.GradeId, s.SubjectName, g.GradeValue, g.GradeDate, u.FullName AS TeacherName, g.Comment
                    FROM Grades g
                    JOIN Subjects s ON g.SubjectId = s.SubjectId
                    JOIN Teachers t ON g.TeacherId = t.TeacherId
                    JOIN Users u ON t.UserId = u.UserId
                    WHERE g.StudentId = @StudentId
                    ORDER BY g.GradeDate DESC", conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@Count", count);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        grades.Add($"{reader["GradeId"]}|{reader["SubjectName"]}|{reader["GradeValue"]}|{Convert.ToDateTime(reader["GradeDate"]):yyyy-MM-dd}|{reader["TeacherName"]}|{reader["Comment"]}");
                    }
                }
            }
            return string.Join(";", grades);
        }

        public string GetAllGrades(int studentId)
        {
            var grades = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT g.GradeId, s.SubjectName, g.GradeValue, g.GradeDate, u.FullName AS TeacherName, g.Comment
                    FROM Grades g
                    JOIN Subjects s ON g.SubjectId = s.SubjectId
                    JOIN Teachers t ON g.TeacherId = t.TeacherId
                    JOIN Users u ON t.UserId = u.UserId
                    WHERE g.StudentId = @StudentId
                    ORDER BY g.GradeDate DESC", conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        grades.Add($"{reader["GradeId"]}|{reader["SubjectName"]}|{reader["GradeValue"]}|{Convert.ToDateTime(reader["GradeDate"]):yyyy-MM-dd}|{reader["TeacherName"]}|{reader["Comment"]}");
                    }
                }
            }
            return string.Join(";", grades);
        }


        public string GetTodaySchedule(int classId)
        {
            var schedule = new List<string>();
            int today = (int)DateTime.Now.DayOfWeek;
            if (today == 0) today = 7;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT sched.ScheduleId, sub.SubjectName, u.FullName AS TeacherName, 
                           sched.StartTime, sched.EndTime, sched.Room
                    FROM Schedule sched
                    JOIN Subjects sub ON sched.SubjectId = sub.SubjectId
                    JOIN Teachers t ON sched.TeacherId = t.TeacherId
                    JOIN Users u ON t.UserId = u.UserId
                    WHERE sched.ClassId = @ClassId AND sched.DayOfWeek = @DayOfWeek
                    ORDER BY sched.StartTime", conn);
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@DayOfWeek", today);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schedule.Add($"{reader["ScheduleId"]}|{reader["SubjectName"]}|{reader["TeacherName"]}|{reader["StartTime"]}|{reader["EndTime"]}|{reader["Room"]}");
                    }
                }
            }
            return string.Join(";", schedule);
        }

        public string GetWeeklySchedule(int classId)
        {
            var schedule = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT sched.ScheduleId, sub.SubjectName, u.FullName AS TeacherName, 
                           sched.StartTime, sched.EndTime, sched.Room, sched.DayOfWeek
                    FROM Schedule sched
                    JOIN Subjects sub ON sched.SubjectId = sub.SubjectId
                    JOIN Teachers t ON sched.TeacherId = t.TeacherId
                    JOIN Users u ON t.UserId = u.UserId
                    WHERE sched.ClassId = @ClassId
                    ORDER BY sched.DayOfWeek, sched.StartTime", conn);
                cmd.Parameters.AddWithValue("@ClassId", classId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schedule.Add($"{reader["ScheduleId"]}|{reader["SubjectName"]}|{reader["TeacherName"]}|{reader["StartTime"]}|{reader["EndTime"]}|{reader["Room"]}|{reader["DayOfWeek"]}");
                    }
                }
            }
            return string.Join(";", schedule);
        }


        public string GetHomeworks(int classId, int count = 3)
        {
            var homeworks = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT TOP (@Count) h.HomeworkId, sub.SubjectName, h.Description, 
                           h.IssueDate, h.DueDate, u.FullName AS TeacherName
                    FROM Homeworks h
                    JOIN Subjects sub ON h.SubjectId = sub.SubjectId
                    JOIN Teachers t ON h.TeacherId = t.TeacherId
                    JOIN Users u ON t.UserId = u.UserId
                    WHERE h.ClassId = @ClassId
                    ORDER BY h.DueDate ASC", conn);
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@Count", count);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        homeworks.Add($"{reader["HomeworkId"]}|{reader["SubjectName"]}|{reader["Description"]}|{Convert.ToDateTime(reader["IssueDate"]):yyyy-MM-dd}|{Convert.ToDateTime(reader["DueDate"]):yyyy-MM-dd}|{reader["TeacherName"]}");
                    }
                }
            }
            return string.Join(";", homeworks);
        }

        public string GetAllHomeworks(int classId)
        {
            var homeworks = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT h.HomeworkId, sub.SubjectName, h.Description, 
                           h.IssueDate, h.DueDate, u.FullName AS TeacherName
                    FROM Homeworks h
                    JOIN Subjects sub ON h.SubjectId = sub.SubjectId
                    JOIN Teachers t ON h.TeacherId = t.TeacherId
                    JOIN Users u ON t.UserId = u.UserId
                    WHERE h.ClassId = @ClassId
                    ORDER BY h.DueDate ASC", conn);
                cmd.Parameters.AddWithValue("@ClassId", classId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        homeworks.Add($"{reader["HomeworkId"]}|{reader["SubjectName"]}|{reader["Description"]}|{Convert.ToDateTime(reader["IssueDate"]):yyyy-MM-dd}|{Convert.ToDateTime(reader["DueDate"]):yyyy-MM-dd}|{reader["TeacherName"]}");
                    }
                }
            }
            return string.Join(";", homeworks);
        }


        public string GetAttendance(int studentId)
        {
            var attendance = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT a.AttendanceId, s.SubjectName, a.AttendanceDate, a.Status, a.Comment
                    FROM Attendance a
                    JOIN Subjects s ON a.SubjectId = s.SubjectId
                    WHERE a.StudentId = @StudentId
                    ORDER BY a.AttendanceDate DESC", conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        attendance.Add($"{reader["AttendanceId"]}|{reader["SubjectName"]}|{Convert.ToDateTime(reader["AttendanceDate"]):yyyy-MM-dd}|{reader["Status"]}|{reader["Comment"]}");
                    }
                }
            }
            return string.Join(";", attendance);
        }


        public string GetTokenTransactions(int studentId)
        {
            var transactions = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT TransactionId, Amount, TransactionType, Reason, 
                           TransactionDate, CurrentBalance
                    FROM TokenTransactions
                    WHERE StudentId = @StudentId
                    ORDER BY TransactionDate DESC", conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add($"{reader["TransactionId"]}|{reader["Amount"]}|{reader["TransactionType"]}|{reader["Reason"]}|{Convert.ToDateTime(reader["TransactionDate"]):yyyy-MM-dd HH:mm}|{reader["CurrentBalance"]}");
                    }
                }
            }
            return string.Join(";", transactions);
        }

        public string AddTokens(int studentId, int amount, string reason)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Получаем текущий баланс 
                        var getCmd = new SqlCommand("SELECT ISNULL(TokenBalance, 0) FROM Students WHERE StudentId = @StudentId", conn, transaction);
                        getCmd.Parameters.AddWithValue("@StudentId", studentId);
                        int currentBalance = Convert.ToInt32(getCmd.ExecuteScalar());

                        int newBalance = currentBalance + amount;

                        // Обновляем баланс 
                        var updateCmd = new SqlCommand("UPDATE Students SET TokenBalance = @NewBalance WHERE StudentId = @StudentId", conn, transaction);
                        updateCmd.Parameters.AddWithValue("@NewBalance", newBalance);
                        updateCmd.Parameters.AddWithValue("@StudentId", studentId);
                        updateCmd.ExecuteNonQuery();

                        // Добавляем транзакцию
                        var transCmd = new SqlCommand(@"
                            INSERT INTO TokenTransactions (StudentId, Amount, TransactionType, Reason, CurrentBalance) 
                            VALUES (@StudentId, @Amount, N'Начисление', @Reason, @CurrentBalance)", conn, transaction);
                        transCmd.Parameters.AddWithValue("@StudentId", studentId);
                        transCmd.Parameters.AddWithValue("@Amount", amount);
                        transCmd.Parameters.AddWithValue("@Reason", reason);
                        transCmd.Parameters.AddWithValue("@CurrentBalance", newBalance);
                        transCmd.ExecuteNonQuery();

                        transaction.Commit();
                        return $"SUCCESS|{newBalance}";
                    }
                    catch
                    {
                        transaction.Rollback();
                        return "ERROR|Не удалось начислить жетоны";
                    }
                }
            }
        }


        public string GetShopItems()
        {
            var items = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ItemId, ItemName, Description, PriceInTokens, Quantity FROM ShopItems WHERE Quantity > 0 ORDER BY PriceInTokens ASC", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add($"{reader["ItemId"]}|{reader["ItemName"]}|{reader["Description"]}|{reader["PriceInTokens"]}|{reader["Quantity"]}");
                    }
                }
            }
            return string.Join(";", items);
        }

        public string GetPurchases(int studentId)
        {
            var purchases = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT p.PurchaseId, si.ItemName, p.PurchaseDate, p.Quantity, p.SpentTokens
                    FROM Purchases p
                    JOIN ShopItems si ON p.ItemId = si.ItemId
                    WHERE p.StudentId = @StudentId
                    ORDER BY p.PurchaseDate DESC", conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        purchases.Add($"{reader["PurchaseId"]}|{reader["ItemName"]}|{Convert.ToDateTime(reader["PurchaseDate"]):yyyy-MM-dd HH:mm}|{reader["Quantity"]}|{reader["SpentTokens"]}");
                    }
                }
            }
            return string.Join(";", purchases);
        }

        public string PurchaseItem(int studentId, int itemId, int quantity)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var itemCmd = new SqlCommand("SELECT PriceInTokens, Quantity FROM ShopItems WHERE ItemId = @ItemId", conn, transaction);
                        itemCmd.Parameters.AddWithValue("@ItemId", itemId);
                        using (var reader = itemCmd.ExecuteReader())
                        {
                            if (!reader.Read()) return "ERROR|Товар не найден";
                            int price = reader.GetInt32(0);
                            int available = reader.GetInt32(1);
                            reader.Close();

                            if (available < quantity) return "ERROR|Недостаточно товара";

                            // Получаем баланс 
                            var balanceCmd = new SqlCommand("SELECT ISNULL(TokenBalance, 0) FROM Students WHERE StudentId = @StudentId", conn, transaction);
                            balanceCmd.Parameters.AddWithValue("@StudentId", studentId);
                            int currentBalance = Convert.ToInt32(balanceCmd.ExecuteScalar());

                            int totalCost = price * quantity;
                            if (currentBalance < totalCost) return "ERROR|Недостаточно жетонов";

                            int newBalance = currentBalance - totalCost;

                            // Обновляем баланс 
                            var updateCmd = new SqlCommand("UPDATE Students SET TokenBalance = @NewBalance WHERE StudentId = @StudentId", conn, transaction);
                            updateCmd.Parameters.AddWithValue("@NewBalance", newBalance);
                            updateCmd.Parameters.AddWithValue("@StudentId", studentId);
                            updateCmd.ExecuteNonQuery();

                            // Транзакция
                            var transCmd = new SqlCommand(@"
                                INSERT INTO TokenTransactions (StudentId, Amount, TransactionType, Reason, CurrentBalance) 
                                VALUES (@StudentId, @Amount, N'Списание', @Reason, @CurrentBalance)", conn, transaction);
                            transCmd.Parameters.AddWithValue("@StudentId", studentId);
                            transCmd.Parameters.AddWithValue("@Amount", -totalCost);
                            transCmd.Parameters.AddWithValue("@Reason", "Покупка товара");
                            transCmd.Parameters.AddWithValue("@CurrentBalance", newBalance);
                            transCmd.ExecuteNonQuery();

                            // Обновляем количество товара
                            var updateItemCmd = new SqlCommand("UPDATE ShopItems SET Quantity = Quantity - @Quantity WHERE ItemId = @ItemId", conn, transaction);
                            updateItemCmd.Parameters.AddWithValue("@Quantity", quantity);
                            updateItemCmd.Parameters.AddWithValue("@ItemId", itemId);
                            updateItemCmd.ExecuteNonQuery();

                            // Запись о покупке
                            var purchaseCmd = new SqlCommand(@"
                                INSERT INTO Purchases (StudentId, ItemId, PurchaseDate, Quantity, SpentTokens) 
                                VALUES (@StudentId, @ItemId, GETDATE(), @Quantity, @SpentTokens)", conn, transaction);
                            purchaseCmd.Parameters.AddWithValue("@StudentId", studentId);
                            purchaseCmd.Parameters.AddWithValue("@ItemId", itemId);
                            purchaseCmd.Parameters.AddWithValue("@Quantity", quantity);
                            purchaseCmd.Parameters.AddWithValue("@SpentTokens", totalCost);
                            purchaseCmd.ExecuteNonQuery();

                            transaction.Commit();
                            return $"SUCCESS|{newBalance}";
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return $"ERROR|{ex.Message}";
                    }
                }
            }
        }


        public string GetTeacherClasses(int teacherId)
        {
            var classes = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT DISTINCT c.ClassId, c.ClassName, c.YearOfStudy,
                           (SELECT COUNT(*) FROM Students s WHERE s.ClassId = c.ClassId) as StudentCount
                    FROM TeacherClassSubject tcs
                    JOIN Classes c ON tcs.ClassId = c.ClassId
                    WHERE tcs.TeacherId = @TeacherId", conn);
                cmd.Parameters.AddWithValue("@TeacherId", teacherId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        classes.Add($"{reader["ClassId"]}|{reader["ClassName"]}|{reader["YearOfStudy"]}|{reader["StudentCount"]}");
                    }
                }
            }
            return string.Join(";", classes);
        }

        public string GetTeacherSubjects(int teacherId)
        {
            var subjects = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT DISTINCT s.SubjectId, s.SubjectName 
                    FROM TeacherClassSubject tcs
                    JOIN Subjects s ON tcs.SubjectId = s.SubjectId
                    WHERE tcs.TeacherId = @TeacherId", conn);
                cmd.Parameters.AddWithValue("@TeacherId", teacherId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subjects.Add($"{reader["SubjectId"]}|{reader["SubjectName"]}");
                    }
                }
            }
            return string.Join(";", subjects);
        }

        public string GetStudentsByClass(int classId, int teacherId)
        {
            var students = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT s.StudentId, u.FullName,
                           ISNULL((SELECT AVG(CAST(g.GradeValue AS FLOAT)) 
                                   FROM Grades g WHERE g.StudentId = s.StudentId), 0) as AvgGrade,
                           ISNULL((SELECT COUNT(*) * 100.0 / 
                                   NULLIF((SELECT COUNT(*) FROM Attendance a2 WHERE a2.StudentId = s.StudentId), 0)
                                   FROM Attendance a WHERE a.StudentId = s.StudentId AND a.Status = N'Присутствовал'), 0) as AttendancePercent,
                           ISNULL((SELECT TokenBalance FROM Students WHERE StudentId = s.StudentId), 0) as TokenBalance
                    FROM Students s
                    JOIN Users u ON s.UserId = u.UserId
                    WHERE s.ClassId = @ClassId", conn);
                cmd.Parameters.AddWithValue("@ClassId", classId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add($"{reader["StudentId"]}|{reader["FullName"]}|{reader["AvgGrade"]}|{reader["AttendancePercent"]}|{reader["TokenBalance"]}");
                    }
                }
            }
            return string.Join(";", students);
        }

        public string AddGrade(int studentId, int teacherId, int subjectId, int gradeValue, string comment)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    INSERT INTO Grades (StudentId, SubjectId, TeacherId, GradeValue, Comment, GradeDate)
                    VALUES (@StudentId, @SubjectId, @TeacherId, @GradeValue, @Comment, GETDATE())", conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                cmd.Parameters.AddWithValue("@GradeValue", gradeValue);
                cmd.Parameters.AddWithValue("@Comment", comment ?? "");

                return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "ERROR|Не удалось добавить оценку";
            }
        }

        public string AddHomework(int subjectId, int teacherId, int classId, string description, DateTime dueDate)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    INSERT INTO Homeworks (SubjectId, TeacherId, ClassId, Description, IssueDate, DueDate)
                    VALUES (@SubjectId, @TeacherId, @ClassId, @Description, GETDATE(), @DueDate)", conn);
                cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                cmd.Parameters.AddWithValue("@ClassId", classId);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@DueDate", dueDate);

                return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "ERROR|Не удалось добавить ДЗ";
            }
        }

        public string AddAttendance(int studentId, int subjectId, string status)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    INSERT INTO Attendance (StudentId, SubjectId, AttendanceDate, Status)
                    VALUES (@StudentId, @SubjectId, GETDATE(), @Status)", conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                cmd.Parameters.AddWithValue("@Status", status);

                return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "ERROR|Не удалось добавить посещаемость";
            }
        }

        public string GetChildByParentId(int parentId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT s.StudentId, s.UserId, s.ClassId, s.BirthDate, s.ParentId,
                           u.FullName, c.ClassName
                    FROM Students s
                    JOIN Users u ON s.UserId = u.UserId
                    JOIN Classes c ON s.ClassId = c.ClassId
                    WHERE s.ParentId = @ParentId", conn);
                cmd.Parameters.AddWithValue("@ParentId", parentId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return $"{reader["StudentId"]}|{reader["FullName"]}|{reader["ClassName"]}|{reader["ClassId"]}|{reader["BirthDate"]}|{reader["ParentId"]}";
                    }
                }
            }
            return "ERROR|Ребенок не найден";
        }

        public string GetUsersWithRoles()
        {
            var users = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT u.UserId, u.Login, u.FullName, u.IsActive, r.RoleName,
                           c.ClassName, s.SubjectName AS Specialty
                    FROM Users u
                    JOIN Roles r ON u.RoleId = r.RoleId
                    LEFT JOIN Students st ON u.UserId = st.UserId
                    LEFT JOIN Classes c ON st.ClassId = c.ClassId
                    LEFT JOIN Teachers t ON u.UserId = t.UserId
                    LEFT JOIN Subjects s ON t.Specialty = s.SubjectName", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add($"{reader["UserId"]}|{reader["Login"]}|{reader["FullName"]}|{reader["RoleName"]}|{reader["IsActive"]}|{reader["ClassName"]}|{reader["Specialty"]}");
                    }
                }
            }
            return string.Join(";", users);
        }

        public string GetClasses()
        {
            var classes = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ClassId, ClassName, YearOfStudy FROM Classes", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        classes.Add($"{reader["ClassId"]}|{reader["ClassName"]}|{reader["YearOfStudy"]}");
                    }
                }
            }
            return string.Join(";", classes);
        }

        public string GetStudents()
        {
            var students = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT s.StudentId, u.FullName, c.ClassName, s.BirthDate, u.Login
                    FROM Students s
                    JOIN Users u ON s.UserId = u.UserId
                    JOIN Classes c ON s.ClassId = c.ClassId", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add($"{reader["StudentId"]}|{reader["FullName"]}|{reader["ClassName"]}|{Convert.ToDateTime(reader["BirthDate"]):yyyy-MM-dd}|{reader["Login"]}");
                    }
                }
            }
            return string.Join(";", students);
        }

        public string GetTeachers()
        {
            var teachers = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT t.TeacherId, u.FullName, t.Specialty, u.Login
                    FROM Teachers t
                    JOIN Users u ON t.UserId = u.UserId", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        teachers.Add($"{reader["TeacherId"]}|{reader["FullName"]}|{reader["Specialty"]}|{reader["Login"]}");
                    }
                }
            }
            return string.Join(";", teachers);
        }

        public string GetSubjects()
        {
            var subjects = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT SubjectId, SubjectName FROM Subjects", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subjects.Add($"{reader["SubjectId"]}|{reader["SubjectName"]}");
                    }
                }
            }
            return string.Join(";", subjects);
        }

        public string GetSchedule()
        {
            var schedule = new List<string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT s.ScheduleId, c.ClassName, sub.SubjectName, 
                           u.FullName AS TeacherName, s.DayOfWeek, 
                           s.StartTime, s.EndTime, s.Room
                    FROM Schedule s
                    JOIN Classes c ON s.ClassId = c.ClassId
                    JOIN Subjects sub ON s.SubjectId = sub.SubjectId
                    JOIN Teachers t ON s.TeacherId = t.TeacherId
                    JOIN Users u ON t.UserId = u.UserId
                    ORDER BY s.DayOfWeek, s.StartTime", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schedule.Add($"{reader["ScheduleId"]}|{reader["ClassName"]}|{reader["SubjectName"]}|{reader["TeacherName"]}|{reader["DayOfWeek"]}|{reader["StartTime"]}|{reader["EndTime"]}|{reader["Room"]}");
                    }
                }
            }
            return string.Join(";", schedule);
        }

        public string CreateUser(string login, string passwordHash, string fullName, string roleName)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var roleCmd = new SqlCommand("SELECT RoleId FROM Roles WHERE RoleName = @RoleName", conn);
                roleCmd.Parameters.AddWithValue("@RoleName", roleName);
                var roleId = roleCmd.ExecuteScalar();
                if (roleId == null) return "ERROR|Роль не найдена";

                var cmd = new SqlCommand(@"
                    INSERT INTO Users (Login, PasswordHash, FullName, RoleId, IsActive) 
                    VALUES (@Login, @PasswordHash, @FullName, @RoleId, 1);
                    SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@Login", login);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@RoleId", roleId);

                int userId = Convert.ToInt32(cmd.ExecuteScalar());
                return userId > 0 ? $"SUCCESS|{userId}" : "ERROR|Не удалось создать пользователя";
            }
        }

        public string CreateClass(string className, int yearOfStudy)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    INSERT INTO Classes (ClassName, YearOfStudy) 
                    VALUES (@ClassName, @YearOfStudy);
                    SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@ClassName", className);
                cmd.Parameters.AddWithValue("@YearOfStudy", yearOfStudy);

                int classId = Convert.ToInt32(cmd.ExecuteScalar());
                return classId > 0 ? $"SUCCESS|{classId}" : "ERROR|Не удалось создать класс";
            }
        }

        public string DeleteUser(int userId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Users WHERE UserId = @UserId", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "ERROR|Не удалось удалить пользователя";
            }
        }

        public string DeleteClass(int classId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Classes WHERE ClassId = @ClassId", conn);
                cmd.Parameters.AddWithValue("@ClassId", classId);
                return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "ERROR|Не удалось удалить класс";
            }
        }

        public string DeleteSubject(int subjectId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Subjects WHERE SubjectId = @SubjectId", conn);
                cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "ERROR|Не удалось удалить предмет";
            }
        }

        public int? GetStudentIdByUserId(int userId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT StudentId FROM Students WHERE UserId = @UserId", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : (int?)null;
            }
        }

        public int? GetClassIdByStudentId(int studentId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ClassId FROM Students WHERE StudentId = @StudentId", conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : (int?)null;
            }
        }

        public int? GetTeacherIdByUserId(int userId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT TeacherId FROM Teachers WHERE UserId = @UserId", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : (int?)null;
            }
        }

        // Получение баланса жетонов ученика 
        public int GetStudentTokenBalance(int studentId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ISNULL(TokenBalance, 0) FROM Students WHERE StudentId = @StudentId", conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
    }
}