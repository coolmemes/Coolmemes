using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Chat
{
    class ChatSettings
    {
        /// <summary>
        /// Código HTML para el color de texto.
        /// </summary>
        private string Color;

        /// <summary>
        /// Tamaño del font.
        /// </summary>
        private uint Size;

        /// <summary>
        /// Si está en negrita.
        /// </summary>
        private bool Bold;

        /// <summary>
        /// Si está en cursiva.
        /// </summary>
        private bool Italics;

        /// <summary>
        /// Si está en subrayado.
        /// </summary>
        private bool Underlined;

        public ChatSettings(string color, uint size, bool bold, bool italics, bool underlined)
        {
            this.Color = color;
            this.Size = size;
            this.Bold = bold;
            this.Italics = italics;
            this.Underlined = underlined;
        }

        public string GetColor()
        {
            return Color;
        }

        public uint GetSize()
        {
            return Size;
        }

        public bool GetBold()
        {
            return Bold;
        }

        public bool GetItalics()
        {
            return Italics;
        }

        public bool GetUnderlined()
        {
            return Underlined;
        }
    }
}
