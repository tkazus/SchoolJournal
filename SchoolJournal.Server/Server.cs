using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SchoolJournal.Server
{
    public class TcpServer
    {
        private TcpListener _listener;
        private Database _db = new Database();
        private bool _isRunning = true;
        private readonly int _port;

        public TcpServer(int port = 8888)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        public void Start()
        {
            try
            {
                _listener.Start();
                Console.WriteLine($" Сервер запущен на порту {_port}");
                Console.WriteLine($" {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                Console.WriteLine(" Ожидание подключений");

                while (_isRunning)
                {
                    try
                    {
                        var client = _listener.AcceptTcpClient();
                        var clientEndPoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
                        Console.WriteLine($" Подключен клиент: {clientEndPoint}");

                        ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClient), client);
                    }
                    catch (Exception ex)
                    {
                        if (_isRunning)
                            Console.WriteLine($" Ошибка при принятии подключения: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка запуска сервера: {ex.Message}");
            }
        }

        private void HandleClient(object? obj)
        {
            if (obj == null) return; 

            var client = (TcpClient)obj;
            var stream = client.GetStream();
            var buffer = new byte[8192];

            try
            {
                while (_isRunning && client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($" Запрос: {request}");

                    string response = ProcessRequest(request);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка клиента: {ex.Message}");
            }
            finally
            {
                try
                {
                    var endPoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
                    Console.WriteLine($" Клиент отключен: {endPoint}");
                    client.Close();
                }
                catch { }
            }
        }

        private string ProcessRequest(string request)
        {
            try
            {
                string[] parts = request.Split('|');
                string command = parts[0];

                Console.WriteLine($"🔍 Команда: {command}");

                switch (command)
                {
                    case "LOGIN":
                        return parts.Length >= 3 ? _db.LoginUser(parts[1], parts[2]) : "ERROR|Недостаточно параметров";

                    case "GET_USER_ROLE":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int uid) ? _db.GetUserRole(uid) : "ERROR|Неверный ID";

                    case "GET_STATISTICS":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int sid) ? _db.GetStatistics(sid) : "ERROR|Неверный ID студента";

                    case "GET_RECENT_GRADES":
                        if (parts.Length >= 2 && int.TryParse(parts[1], out int rsid))
                        {
                            int count = parts.Length >= 3 && int.TryParse(parts[2], out int c) ? c : 5;
                            return _db.GetRecentGrades(rsid, count);
                        }
                        return "ERROR|Неверный ID студента";

                    case "GET_ALL_GRADES":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int gsid) ? _db.GetAllGrades(gsid) : "ERROR|Неверный ID студента";

                    case "GET_TODAY_SCHEDULE":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int tcid) ? _db.GetTodaySchedule(tcid) : "ERROR|Неверный ID класса";

                    case "GET_WEEKLY_SCHEDULE":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int wcid) ? _db.GetWeeklySchedule(wcid) : "ERROR|Неверный ID класса";

                    case "GET_HOMEWORKS":
                        if (parts.Length >= 2 && int.TryParse(parts[1], out int hcid))
                        {
                            int count = parts.Length >= 3 && int.TryParse(parts[2], out int hc) ? hc : 3;
                            return _db.GetHomeworks(hcid, count);
                        }
                        return "ERROR|Неверный ID класса";

                    case "GET_ALL_HOMEWORKS":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int allHwClassId) ? _db.GetAllHomeworks(allHwClassId) : "ERROR|Неверный ID класса";

                    case "GET_ATTENDANCE":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int asid) ? _db.GetAttendance(asid) : "ERROR|Неверный ID студента";

                    case "GET_TOKEN_TRANSACTIONS":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int tsid) ? _db.GetTokenTransactions(tsid) : "ERROR|Неверный ID студента";

                    case "ADD_TOKENS":
                        if (parts.Length >= 4 && int.TryParse(parts[1], out int atsid) && int.TryParse(parts[2], out int amount))
                        {
                            return _db.AddTokens(atsid, amount, parts[3]);
                        }
                        return "ERROR|Неверный формат запроса";

                    case "GET_SHOP_ITEMS":
                        return _db.GetShopItems();

                    case "GET_PURCHASES":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int psid) ? _db.GetPurchases(psid) : "ERROR|Неверный ID студента";

                    case "PURCHASE_ITEM":
                        if (parts.Length >= 4 && int.TryParse(parts[1], out int pistudentId) &&
                            int.TryParse(parts[2], out int piitemId) && int.TryParse(parts[3], out int piquantity))
                        {
                            return _db.PurchaseItem(pistudentId, piitemId, piquantity);
                        }
                        return "ERROR|Неверный формат запроса";

                    case "GET_TEACHER_CLASSES":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int tchid) ? _db.GetTeacherClasses(tchid) : "ERROR|Неверный ID учителя";

                    case "GET_TEACHER_SUBJECTS":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int tchsubid) ? _db.GetTeacherSubjects(tchsubid) : "ERROR|Неверный ID учителя";

                    case "GET_STUDENTS_BY_CLASS":
                        if (parts.Length >= 3 && int.TryParse(parts[1], out int sbcid) && int.TryParse(parts[2], out int sbtid))
                        {
                            return _db.GetStudentsByClass(sbcid, sbtid);
                        }
                        return "ERROR|Неверные параметры";

                    case "ADD_GRADE":
                        if (parts.Length >= 6 && int.TryParse(parts[1], out int agsid) && int.TryParse(parts[2], out int agtid) &&
                            int.TryParse(parts[3], out int agsubid) && int.TryParse(parts[4], out int agval))
                        {
                            return _db.AddGrade(agsid, agtid, agsubid, agval, parts.Length > 5 ? parts[5] : "");
                        }
                        return "ERROR|Неверный формат запроса";

                    case "ADD_HOMEWORK":
                        if (parts.Length >= 6 && int.TryParse(parts[1], out int ahsubid) && int.TryParse(parts[2], out int ahtid) &&
                            int.TryParse(parts[3], out int ahcid) && DateTime.TryParse(parts[5], out DateTime ahdate))
                        {
                            return _db.AddHomework(ahsubid, ahtid, ahcid, parts[4], ahdate);
                        }
                        return "ERROR|Неверный формат запроса";

                    case "ADD_ATTENDANCE":
                        if (parts.Length >= 4 && int.TryParse(parts[1], out int aastudentid) && int.TryParse(parts[2], out int aasubjectid))
                        {
                            return _db.AddAttendance(aastudentid, aasubjectid, parts[3]);
                        }
                        return "ERROR|Неверный формат запроса";

                    case "GET_CHILD":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int ppid) ? _db.GetChildByParentId(ppid) : "ERROR|Неверный ID родителя";

                    case "GET_USERS":
                        return _db.GetUsersWithRoles();

                    case "GET_CLASSES":
                        return _db.GetClasses();

                    case "GET_STUDENTS_LIST":
                        return _db.GetStudents();

                    case "GET_TEACHERS_LIST":
                        return _db.GetTeachers();

                    case "GET_SUBJECTS":
                        return _db.GetSubjects();

                    case "GET_SCHEDULE":
                        return _db.GetSchedule();

                    case "CREATE_USER":
                        if (parts.Length >= 5)
                            return _db.CreateUser(parts[1], parts[2], parts[3], parts[4]);
                        return "ERROR|Неверный формат запроса";

                    case "CREATE_CLASS":
                        if (parts.Length >= 3 && int.TryParse(parts[2], out int cyear))
                            return _db.CreateClass(parts[1], cyear);
                        return "ERROR|Неверный формат запроса";

                    case "DELETE_USER":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int duid) ? _db.DeleteUser(duid) : "ERROR|Неверный ID";

                    case "DELETE_CLASS":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int dcid) ? _db.DeleteClass(dcid) : "ERROR|Неверный ID";

                    case "DELETE_SUBJECT":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int dsid) ? _db.DeleteSubject(dsid) : "ERROR|Неверный ID";

                    case "GET_STUDENT_ID":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int gsiuid) ?
                            (_db.GetStudentIdByUserId(gsiuid)?.ToString() ?? "0") : "ERROR|Неверный ID";

                    case "GET_CLASS_ID":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int gcisid) ?
                            (_db.GetClassIdByStudentId(gcisid)?.ToString() ?? "0") : "ERROR|Неверный ID";

                    case "GET_TEACHER_ID":
                        return parts.Length >= 2 && int.TryParse(parts[1], out int gtuid) ?
                            (_db.GetTeacherIdByUserId(gtuid)?.ToString() ?? "0") : "ERROR|Неверный ID";

                    case "PING":
                        return "PONG";

                    default:
                        return $"ERROR|Неизвестная команда: {command}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка обработки запроса: {ex.Message}");
                return $"ERROR|{ex.Message}";
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
            Console.WriteLine(" Сервер остановлен");
        }
    }
}