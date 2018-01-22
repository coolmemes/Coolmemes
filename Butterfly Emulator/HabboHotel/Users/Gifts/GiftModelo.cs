using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Gifts
{
    class GiftModelo
    {
        internal string imagem;
        internal uint identificador;
        internal string nomeItem;
        internal int quantidadeItensDoPacote;

        public GiftModelo(string imagem, uint identificador, string nomeItem)
        {
            this.imagem = imagem;
            this.identificador = identificador;
            this.nomeItem = nomeItem;
            quantidadeItensDoPacote = nomeItem.Split('/').Count();
        }
    }
}
