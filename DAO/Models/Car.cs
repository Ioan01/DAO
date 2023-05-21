using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DAO.Models
{
    internal class Car
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Hp { get; set; }

        public double Speed { get; set; }

        public bool Cool { get; set; }


        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
