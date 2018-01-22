using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Prisao
{
    class PrisaoUsuario
    {
        internal uint userid;
        internal double tempoRestante;
        public PrisaoUsuario(uint userid, double tempoRestante)
        {
            this.userid = userid;
            this.tempoRestante = tempoRestante;
        }
    }
}
