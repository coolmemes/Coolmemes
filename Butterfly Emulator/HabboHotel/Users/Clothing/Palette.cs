using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Clothing
{
    class Palette
    {
        /// <summary>
        /// Número de paleta donde se encuentra el color.
        /// </summary>
        public int PaletteId;

        /// <summary>
        /// Id del color.
        /// </summary>
        public int Id;

        /// <summary>
        /// Orden de aparición del color.
        /// </summary>
        public int Index;

        /// <summary>
        /// Este color es solo para gente del Club.
        /// </summary>
        public int Club;

        /// <summary>
        /// Si se puede seleccionar el botón (activado)
        /// </summary>
        public bool Selectable;

        public Palette(int paletteId, int id, int index, int club, bool selectable)
        {
            this.PaletteId = paletteId;
            this.Id = id;
            this.Index = index;
            this.Club = club;
            this.Selectable = selectable;
        }


    }
}
