using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Clothing
{
    class Clothes
    {
        public string Type;
        public int PaletteId;
        public int Id;
        public string Gender;
        public int Club;
        public bool Colorable;
        public bool Sellable;

        public Clothes(string Type, int PaletteId, int Id, string Gender, int Club, bool Colorable, bool Sellable)
        {
            this.Type = Type;
            this.PaletteId = PaletteId;
            this.Id = Id;
            this.Gender = Gender;
            this.Club = Club;
            this.Colorable = Colorable;
            this.Sellable = Sellable;
        }
    }
}
