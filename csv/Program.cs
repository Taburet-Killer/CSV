using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Dynamic;
using System.Net.Cache;
using System.IO.Compression;

namespace csv
{
    class Program
    {




        static void Main(string[] args)
        {

            Console.Title = "CSV";
            
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
                        Console.WriteLine("mkrepo - создаёт репозиторий");
                        Console.WriteLine("mkdata - создаёт базу данных репозитория");
                        Console.WriteLine("dir - отображает файлы в репозитории");
                        Console.WriteLine("cls - очищает консоль");
                        Console.WriteLine("save - загружает файлы в базу данных");
                        Console.WriteLine("load - выгружает файлы из базы данных в репозиторий");
                        Console.WriteLine("info - выводит информацию из базы данных");
                        break;
                    case "mkrepo":
                        NavigateDirectories();
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
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\temp");
            ZipFile.CreateFromDirectory(File.ReadAllText(Directory.GetCurrentDirectory() + "\\" + "data" + "\\" + "repo.path"),"temp\\repo.zip");
            
            byte[] fileData = File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\temp\\repo.zip");
            string fileName = Path.GetFileName(Directory.GetCurrentDirectory() + "\\temp\\repo.zip");
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
            Directory.Delete(Directory.GetCurrentDirectory() + "\\temp", true);
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
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\temp");

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


                        File.WriteAllBytes(Directory.GetCurrentDirectory() + "\\" + "temp" + "\\" + fileName, fileData);
                    }
                    

                    if (!found)
                    {
                        Console.WriteLine("Файлы не найдены по указанному комментарию.");
                    }




                }



            }
            DirectoryInfo folder = new DirectoryInfo(File.ReadAllText(Directory.GetCurrentDirectory() + "\\data\\repo.path"));

            foreach (FileInfo file in folder.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in folder.GetDirectories())
            {
                dir.Delete(true);
            }
            ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + "\\" + "temp" + "\\" + "repo.zip", File.ReadAllText(Directory.GetCurrentDirectory() + "\\data\\repo.path"));
            Directory.Delete(Directory.GetCurrentDirectory() + "\\temp", true);
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

        static void NavigateDirectories()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ВНИМАНИЕ ФУНКЦИЯ НЕДОРАБОТАНА И УДАЛЕНИЯ РЕПОЗИТОРИЯ ПРИХОДИТЬСЯ ДЕЛАТЬ ВРУЧНУЮ ");
            Console.WriteLine("ТАК ЖЕ ВОЗМОЖНО СОЗДАТЬ РЕПОЗИТОРИЙ ТОЛЬКО НА ОСНОВНОМ ДИСКЕ");
            Console.Beep(400, 2000);
            Console.ResetColor();
            Stack<string> directoryStack = new Stack<string>();
            string currentDirectory = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()); // Начало с корня текущего диска
            int selectedIndex = 0;

            while (!File.Exists(Directory.GetCurrentDirectory() + "\\" + "data" + "\\" + "repo.path"))
            {
                Console.Clear();
                string[] directories = Directory.GetDirectories(currentDirectory);
                List<string> items = new List<string>(directories);

                // Добавляем пункт "Назад", если есть куда вернуться
                if (directoryStack.Count > 0)
                {
                    items.Insert(0, ".. (Назад)");
                }

                // Выводим доступные директории
                for (int i = 0; i < items.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(items[i]);
                    Console.ResetColor();
                    
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("↑ - вверх | ↓ -  вниз | → - создать репозиторий | ENTER - выбрать директорию");
                Console.ResetColor();
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : items.Count - 1;
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex < items.Count - 1) ? selectedIndex + 1 : 0;
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                        Directory.CreateDirectory(currentDirectory + "\\" + "Repository");
                        File.AppendAllText(Directory.GetCurrentDirectory() + "\\" + "data" + "\\" + "repo.path", currentDirectory + "\\" + "Repository");
                        Console.WriteLine($"Путь '{currentDirectory}' записан в файл 'repo.path'. Нажмите любую клавишу для продолжения...");
                        Console.ReadKey();
                        Console.Clear();
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    if (selectedIndex == 0 && directoryStack.Count > 0)
                    {
                        // Переход назад
                        currentDirectory = directoryStack.Pop();
                        selectedIndex = 0; // Сбросить индекс при возврате
                    }
                    else
                    {
                        string selectedPath = items[selectedIndex];

                        // Проверяем, является ли элемент директорией
                        if (Directory.Exists(selectedPath))
                        {
                            // Переход в выбранную директорию
                            directoryStack.Push(currentDirectory);
                            currentDirectory = selectedPath;
                            selectedIndex = 0; // Сбросить индекс при переходе в папку
                        }
                    }
                }
            }
        }
    }
}



