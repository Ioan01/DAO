using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAO.Models;

namespace DAO
{
    internal interface CarRepository
    {
        public bool saveCar(Car car);

        public IEnumerable<Car> findAll();

        public Car findById(int id);

        //public Car findByNameAndHp(string name, int hp);

        public IEnumerable<Car> findByNameAndHp(string name, int hp);

        public IEnumerable<Car> findByNameAndSpeedAndHp(string name, double speed,int hp);

        public void deleteByNameAndHp(string name, int hp);

        public void deleteAll();

    }
}
