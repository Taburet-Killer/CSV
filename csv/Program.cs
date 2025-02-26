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
        public static void Ceaterepo()
        {
            if (!File.Exists("D:\\Database\\Repo.db"))
            {
                File.Create("D:\\Database\\Repo.db");
                Console.WriteLine("repo create");
            }
            else { Console.WriteLine("sasi repo est"); }
            Console.ReadKey();
        }
        public static void Command(string input)
        {
            try
            {
                switch (input)
                {
                    case "dir":
                        var dir  = Directory.GetDirectories(Directory.GetCurrentDirectory());
                        var file = Directory.GetFiles(Directory.GetCurrentDirectory());
                        foreach (var dirs in dir )
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
                    case "mkrepo":

                        string cdir = Directory.GetCurrentDirectory();
                        Directory.CreateDirectory(cdir + "\\" +"Repository");
                        if (!File.Exists(cdir + "\\" + "Repository" + "\\"  + "Repository.db"))
                        {
                            File.Create(cdir + "\\" + "Repository.db");
                            Console.WriteLine("Repository Database created");
                        }
                        else
                        {
                            Console.WriteLine("Repository Database is exists");
                        }
                        break;
                    case "cd ..":
                        Console.WriteLine("NOT WORKING");
                        break;
                }
            }
            catch
            {
                Console.WriteLine("invalid syntax:");
            }
        }
    }
}
