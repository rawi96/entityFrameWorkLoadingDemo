using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EfLoadingDemo
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
    }

    public class Reservation
    {
        public int Id { get; set; }
        public virtual Car Car { get; set; }
        public virtual Customer Customer { get; set; }
    }

    public class Context : DbContext
    {
        public virtual DbSet<Car> Cars { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Reservation> Reservations { get; set; }
        private readonly StreamWriter _logStream = new StreamWriter(@"c:\tmp\log.txt", append: true);


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=.; Database=Loading; Trusted_Connection=True");
            optionsBuilder.LogTo(_logStream.WriteLine);
        }

        public override void Dispose()
        {
            base.Dispose();
            _logStream.Dispose();
        }
    }



    class Program
    {
        static void Main(string[] args)
        {
            LoadData();

            using (var context = new Context())
            {
                var autos = context.Cars.ToList();

                foreach (var auto in autos)
                {
                    Console.WriteLine(auto.Brand);
                    foreach (var reservation in auto.Reservations)
                    {
                        Console.WriteLine(reservation.Customer.LastName);
                    }
                }
            }
            Console.ReadLine();
        }


        private static void LoadData()
        {
            var volvo = new Car { Brand = "Volvo" };
            var mazda = new Car { Brand = "Mazda" };
            var vw = new Car { Brand = "VW" };
            
            var wirth = new Customer { LastName = "Wirth" };
            var koenig = new Customer { LastName = "König" };
            var wenger = new Customer { LastName = "Wenger" };

            var montag = new Reservation { Customer = wirth, Car = volvo };
            var dienstag = new Reservation { Customer = koenig, Car = mazda };
            var mittwoch = new Reservation {  Customer = wenger, Car = vw };

            using (var context = new Context())
            {
                context.Cars.AddRange(volvo, mazda, vw);
                context.Customers.AddRange(wirth, koenig, wenger);
                context.Reservations.AddRange(montag, dienstag, mittwoch);
                context.SaveChanges();
            }
        }
    }
}
