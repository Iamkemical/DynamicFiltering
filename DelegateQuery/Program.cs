using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DelegateQuery
{
    class Program
    {
        private static readonly List<Employee> output = EmployeeDataSeed();
        static void Main(string[] args)
        {
            Console.Write("Specify Employee property to filter: ");
            var propertyName = Console.ReadLine();

            Console.Write("Specify the Value of the property: ");
            var propertyValue = Console.ReadLine();

            // 1. Uses Func<> predicate for dynamic filtering
            var dynamicExpression = GetDynamicQueryFromFunc(propertyName, propertyValue);
            var results = output.Where(dynamicExpression).ToList();

            // 2. Uses Expression Trees for dynamic filtering
            //var dynamicExpressionTree = GetDynamicQueryWithExpressionTree(propertyName, propertyValue);
            //var results = output.Where(dynamicExpressionTree).ToList();


            var queryResult = from result in results
                              orderby result ascending
                              select result;

            foreach (var query in queryResult)
            {
                Console.WriteLine();
                Console.WriteLine($"Name: {query.Name}");
                Console.WriteLine($"Age: {query.Age}");
                Console.WriteLine($"Job Description: {query.JobDescription}");
                Console.WriteLine($"Salary: {query.Salary}");
            }
        }

       

        private static Func<Employee, bool> GetDynamicQueryFromFunc(string propName, object val)
        {
            Func<Employee, bool> employee = (e) => true;
            switch (propName.ToLower())
            {
                case "name":
                    employee = d => d.Name == Convert.ToString(val);
                    break;
                case "age":
                    employee = d => d.Age == Convert.ToUInt32(val);
                    break;
                case "jobdescription":
                    employee = d => d.JobDescription == Convert.ToString(val);
                    break;
                case "salary":
                    employee = d => d.Salary == Convert.ToDouble(val);
                    break;
                default:
                    break;
            }
            return employee;
        }

        private static List<Employee> EmployeeDataSeed()
        {
            var employeeDataResult = new List<Employee>();

            employeeDataResult.Add(new() { Name = "John", Age = 21, JobDescription = "Developer", Salary = 100000 });
            employeeDataResult.Add(new() { Name = "Ezekiel", Age = 25, JobDescription = "ML Engineer", Salary = 150000 });
            employeeDataResult.Add(new() { Name = "Ada", Age = 25, JobDescription = "Designer", Salary = 250000 });
            employeeDataResult.Add(new() { Name = "Princess", Age = 27, JobDescription = "Venture Capitalist", Salary = 120000 });

            return employeeDataResult;
        }

        private static Func<Employee, bool> GetDynamicQueryWithExpressionTree(string propertyName, string val)
        {
            // x =>
            var param = Expression.Parameter(typeof(Employee), "x");

            #region Convert to specific data type
            // x => x.PropertyName
            MemberExpression member = Expression.Property(param, propertyName);

            // x => x.PropertyName == val
            UnaryExpression valExpression = GetValueExpression(propertyName, val, param);
            #endregion

            // x => x.PropertyName == val is true
            Expression body = Expression.Equal(member, valExpression);

            var final = Expression.Lambda<Func<Employee, bool>>(body, param);

            return final.Compile();
        }

        private static UnaryExpression GetValueExpression(string propName, string val, ParameterExpression param)
        {
            var member = Expression.Property(param, propName);

            var propertyType = ((PropertyInfo)member.Member).PropertyType;

            var converter = TypeDescriptor.GetConverter(propertyType);

            if (!converter.CanConvertFrom(typeof(string)))
                throw new NotSupportedException();

            var propertyValue = converter.ConvertFromInvariantString(val);

            var constant = Expression.Constant(propertyValue);

            return Expression.Convert(constant, propertyType);
        }
    }
}