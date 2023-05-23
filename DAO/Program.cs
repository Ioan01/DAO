using DAO.Models;

namespace DAO
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Database database =
                new Database(
                    "Server=127.0.0.1;Port=5432;Database=test;User Id=postgres;Password=postgres;","schema_name");

            var repo = database.CreateRepository<CarRepository>();

            // repo.saveCar(new Car(){Name = "benveu",Hp = 200,Cool = true,Speed = 300.9});
            // repo.saveCar(new Car() { Name = "benveu seria septe", Hp = 200, Cool = true, Speed = 300.9 });
            //
            //
            //
            //
            // Console.WriteLine(repo.findById(22));
            //
            // foreach (var car in repo.findByNameAndHp("benveu",200))
            // {
            //     Console.WriteLine(car);
            // }
            //
            // repo.deleteByNameAndHp("benveu",200);
            //
            // foreach (var car in repo.findByNameAndHp("benveu", 200))
            // {
            //     Console.WriteLine(car);
            // }
            //
            // Console.WriteLine(repo.findByNameAndSpeedAndHp("benveu seria septe",300.9,200));
            //
            //
            // repo.deleteAll();
            //
            // var list = repo.findAll();
            //
            // foreach (var car in list)
            // {
            //     Console.WriteLine(car);
            // }

            repo.deleteByNameAndHp("benveu",200);
            repo.deleteAll();

            repo.saveCar(new Car() { Cool = true, Hp = 500, Name = "Benveu", Speed = 300 });

            repo.findByNameAndSpeedAndHp("benveu", 200, 300);


        }
    }
}