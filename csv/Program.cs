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
                        var dir = Directory.GetDirectories(Directory.GetCurrentDirectory() + "\\data");
                        var file = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Repository");
                        foreach (var dirs in dir)
                        {
                            DirectoryInfo dirinfo = new DirectoryInfo(dirs);
                            Console.WriteLine(dirinfo.Name);
                        }
                        foreach (var files in file)
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
                    case "Load":


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
            var connection = new SqliteConnection("Data Source=" + Directory.GetCurrentDirectory() + "\\data\\Repository.db");
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
            byte[] fileData = File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\Repository\\1.txt");
            string fileName = Path.GetFileName(Directory.GetCurrentDirectory() + "\\Repository\\1.txt");
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
        }
        static long LastNum(SqliteConnection connection)
        {
            string selectQuery = "SELECT Number FROM UserFiles ORDER BY rowid DESC LIMIT 1;";
            using (var command = new SqliteCommand(selectQuery, connection))
            {
                var result = command.ExecuteScalar();
                return result != null ? (long)result : -1; // Возвращаем -1, если нет записей
            }
        }
        public static void Load()
        {
        }
    }
   
}



