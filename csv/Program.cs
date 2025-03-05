using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;

namespace csv
{
    class Program
    {




        static void Main(string[] args)
        {


            Console.WriteLine("Введите help для просмтора комманд");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(Directory.GetCurrentDirectory() + "   =>");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Green;
                string input = Console.ReadLine().ToLower();
                Command(input);
                Console.ResetColor();
            }
        }
        public static void Command(string input)
        {
            try
            {
                switch (input)
                {
                    case "dir":
                        string[] dir = Directory.GetDirectories(Directory.GetCurrentDirectory() + "\\Repository");
                        string[] file = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Repository");
                        foreach (string dirs in dir)
                        {
                            DirectoryInfo dirinfo = new DirectoryInfo(dirs);
                            Console.WriteLine(dirinfo.Name);
                        }
                        foreach (string files in file)
                        {
                            FileInfo fileInfo = new FileInfo(files);
                            Console.WriteLine(fileInfo.Name);

                        }
                        break;
                    case "cls":
                        Console.Clear();
                        break;
                    case "mkdata":

                        string cdir = Directory.GetCurrentDirectory();
                        if (!File.Exists(cdir + "\\" + "data" + "\\" + "Repository.db"))
                        {
                            Directory.CreateDirectory(cdir + "\\" + "data");
                            Console.WriteLine("База данных репозитория создана");
                            table();
                        }
                        else
                        {
                            Console.WriteLine("База данных репозитория уже существует");
                        }
                        if (!Directory.Exists(cdir + "\\" + "Repository")) { Directory.CreateDirectory(cdir + "\\" + "Repository"); }

                        break;
                    case "save":
                        Save();
                        break;
                    case "load":
                        Load();
                        break;
                    case "info":
                        DisplayInfo();
                        break;
                    case "help":
                        Console.WriteLine("mkdata - создаёт репозиторий, и базу данных репозитория");
                        Console.WriteLine("dir - отображает файлы в репозитории");
                        Console.WriteLine("cls - очищает консоль");
                        Console.WriteLine("save - загружает файлы в базу данных");
                        Console.WriteLine("load - выгружает файлы из базы данных в репозиторий");
                        Console.WriteLine("info - выводит информацию из базы данных");
                        break;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void table()
        {
            SqliteConnection connection = new SqliteConnection("Data Source=" + Directory.GetCurrentDirectory() + "\\data\\Repository.db");
            connection.Open();
            Console.WriteLine();

            string createTableQuery = @"
            CREATE TABLE UserFiles(
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Number INTEGER,
                UserName NVARCHAR(255),
                DateAdded DATE,
                FileData VARBINARY(2147483647),
                FileName NVARCHAR(255),
                Comment NVARCHAR(255)
            );";
            try
            {

                SqliteCommand command = new SqliteCommand(createTableQuery, connection);

                command.ExecuteNonQuery();
                Console.WriteLine("Таблица UserFiles успешно создана.");
                connection.Close();

            }
            catch (SqliteException ex)
            {
                Console.WriteLine("Ошибка при создании таблицы: " + ex.Message);
            }

        }
        public static void Save()
        {
            Console.WriteLine("Введите имя файла для сохранения вместе с расширением");
            string fileadd = Console.ReadLine();
            byte[] fileData = File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\Repository\\" + fileadd );
            string fileName = Path.GetFileName(Directory.GetCurrentDirectory() + "\\Repository\\" + fileadd);
            DateTime date = DateTime.Today;
            SqliteConnection connection = new SqliteConnection("Data Source=" + Directory.GetCurrentDirectory() + "\\data\\Repository.db");
            connection.Open();
            string sql = "INSERT INTO UserFiles(Number, UserName, DateAdded, FileData, FileName, Comment) VALUES (@Number, @UserName, @DateAdded, @FileData, @FileName, @Comment)";
            using (SqliteCommand command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Number", LastNum(connection) + 1);
                command.Parameters.AddWithValue("@UserName", Environment.UserName);
                command.Parameters.AddWithValue("@DateAdded", date.ToString("d"));
                command.Parameters.AddWithValue("@FileData", fileData);
                command.Parameters.AddWithValue("@FileName", fileName);
                Console.WriteLine();
                Console.WriteLine(date.ToString("d"));
                Console.Write("Кометнарий ==>");
                command.Parameters.AddWithValue("@Comment", Console.ReadLine());


                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} row(s) inserted.");
            }
            connection.Close();
        }
        static long LastNum(SqliteConnection connection)
        {
            string selectQuery = "SELECT Number FROM UserFiles ORDER BY rowid DESC LIMIT 1;";
            using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
            {
                var result = command.ExecuteScalar();
                return result != null ? (long)result : -1;
            }
        }
        public static void Load()
        {
            
            
            Console.Write("Введите комментарий: ");
            string comment = Console.ReadLine();
            SqliteConnection connection = new SqliteConnection("Data Source=" + Directory.GetCurrentDirectory() + "\\data\\Repository.db");
                connection.Open();
                string query = "SELECT FileData, FileName FROM UserFiles WHERE Comment LIKE @comment";
                using (SqliteCommand cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@comment", "%" + comment + "%");

                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        bool found = false;
                        while (reader.Read())
                        {
                            found = true;
                            
                            byte[] fileData = (byte[])reader["FileData"];
                            string fileName = reader["FileName"].ToString();


                            File.WriteAllBytes(Directory.GetCurrentDirectory() + "//" + "Repository//" + fileName , fileData);
                        }

                        if (!found)
                        {
                            Console.WriteLine("Файлы не найдены по указанному комментарию.");
                        }




                    }



                }
            
        }
        public static void DisplayInfo()
        {
            SqliteConnection connection = new SqliteConnection("Data Source=" + Directory.GetCurrentDirectory() + "\\data\\Repository.db");
            try
            {
                connection.Open();

                string query = "SELECT UserName, DateAdded, FileName, Comment FROM UserFiles";

                using (SqliteCommand cmd = new SqliteCommand(query, connection))
                {
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("------------------------------------------------------------------------------------------------------");
                        Console.WriteLine("| UserName            | DateAdded             | FileName                      | Comment                       |");
                        Console.WriteLine("------------------------------------------------------------------------------------------------------");

                        while (reader.Read())
                        {
                            string userName = reader["UserName"].ToString().PadRight(20); 
                            DateTime dateAdded = DateTime.Parse(reader["DateAdded"].ToString());
                            string dateAddedFormatted = dateAdded.ToString("yyyy-MM-dd");
                            string fileName = reader["FileName"].ToString().PadRight(30);
                            string comment = reader["Comment"].ToString().PadRight(30);

                            Console.WriteLine($"| {userName} | {dateAddedFormatted} | {fileName} | {comment} |");
                        }
                        Console.WriteLine("------------------------------------------------------------------------------------------------------");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при подключении к базе данных или выполнении запроса: {ex.Message}");
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
    }
}




