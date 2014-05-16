using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace OOP
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public DateTime DateHired { get; set; }

        public bool IsAdult()
        {
            return Age >= 21;
        }
    }

    public interface IDataLoader
    {
        string LoadData();
    }

    public class FileLoader : IDataLoader
    {
        private readonly string _fileName;

        public FileLoader(string fileName)
        {
            _fileName = fileName;
        }


        public string LoadData()
        {
            return File.ReadAllText(_fileName);
        }
    }

    public class WebLoader : IDataLoader
    {
        private readonly string _url;

        public WebLoader(string url)
        {
            _url = url;
        }

        public string LoadData()
        {
            var client = new WebClient();
            return client.DownloadString(new Uri(_url));
        }
    }

    public interface IEmployeeParser
    {
        IList<Employee> ParseEmployees();
    }

    public class EmployeeCsvParser : IEmployeeParser
    {
        private readonly IDataLoader _loader;

        public EmployeeCsvParser(IDataLoader loader)
        {
            _loader = loader;
        }

        public IList<Employee> ParseEmployees()
        {
            var csvData = _loader.LoadData().Split('\n');
            return (
               from line in csvData
               let data = line.Split(',')
               select new Employee()
               {
                   Id = int.Parse(data[0]),
                   FirstName = data[1],
                   LastName = data[2],
                   Age = int.Parse(data[3]),
                   Email = data[4],
                   DateHired = DateTime.Parse(data[5])
               }).ToList();
        }
    }

    public class AdultLocator
    {
        private readonly IList<Employee> _employees;

        public AdultLocator(IList<Employee> employees)
        {
            _employees = employees;
        }

        public IEnumerable<Employee> Locate()
        {
            for (int i = 0; i < _employees.Count; i++)
            {
                if (_employees[i].IsAdult())
                {
                    yield return _employees[i];
                }
            }
        }
    }

    class EmployeeAnalyzer
    {
        private readonly IEmployeeParser _parser;

        public EmployeeAnalyzer(IEmployeeParser parser)
        {
            _parser = parser;
        }

        public IEnumerable<Employee> FindAdults()
        {
            var employees = _parser.ParseEmployees();
            var locator = new AdultLocator(employees);
            return locator.Locate();
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var loader = GetLoaderFor(args[0]);
            var parser = new EmployeeCsvParser(loader);
            var analyzer = new EmployeeAnalyzer(parser);

            foreach (var adult in analyzer.FindAdults())
            {
                PrintEmployee(adult);
            }
            Console.ReadLine();
        }

        private static IDataLoader GetLoaderFor(string source)
        {
            IDataLoader loader;
            if (source.ToLower().StartsWith("http"))
            {
                loader = new WebLoader(source);
            }
            else
            {
                loader = new FileLoader(source);
            }
            return loader;
        }

        private static void PrintEmployee(Employee employee)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Employee #{0}, {1} {2} has reached the age of 21. Can be served alchohol.", employee.Id, employee.FirstName, employee.LastName);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
