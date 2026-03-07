using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishForKids.Data.Entities
{
    public class VirtualPet
    {
        public int Id { get; set; }
        public int ChildProfileId { get; set; }
        public ChildProfile Child { get; set; }
        public string PetType { get; set; } // Дракончик, Енот, Совенок
        public string PetName { get; set; } // Имя, которое даст ребенок
        public int Level { get; set; }
        public int Experience { get; set; } // Опыт
        public int Happiness { get; set; } // 0-100
        public int Food { get; set; } // 0-100
        public int Energy { get; set; } // 0-100
        public string Accessory { get; set; } // Одетая шляпка и т.д.
        public DateTime LastFed { get; set; }
        public DateTime LastPlayed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}