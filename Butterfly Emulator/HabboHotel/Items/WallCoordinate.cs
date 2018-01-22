using System;
namespace Butterfly.HabboHotel.Items
{
    class WallCoordinate
    {
        private int widthX;
        private int widthY;

        private int lengthX;
        private int lengthY;

        private char side;

        public WallCoordinate(string wallPosition)
        {
            // exemplo: :w=7,0 l=2,38 r
            /* string[] posD = 
             *          posD[0] = :w=7,0
             *          posD[1] = l=2,38
             *          posD[2] (toChar) = r
             * 
             * var widD = posD[0].Substring(3).Split(',');
             * widD = :w=7,0
             *            ele começa a partir do = (primeiro caracterer é o 7, então o valor 0 do array é o 7)
             *             widthX = TextHandling.Parse(widD[0]) = 7
             *             widthY = TextHandling.Parse(widD[1]); = 0
             *             
             * var lenD = posD[1].Substring(2).Split(',');
             * lenD = l=2,38
             *             lengthX = TextHandling.Parse(lenD[0]); = 2
             *             lengthY = TextHandling.Parse(lenD[1]); = 38
             *
             * */
            var posD = wallPosition.Split(' ');
            side = posD[2].ToCharArray()[0];

            var widD = posD[0].Substring(3).Split(',');
            widthX = TextHandling.Parse(widD[0]);
            widthY = TextHandling.Parse(widD[1]);

            var lenD = posD[1].Substring(2).Split(',');
            lengthX = TextHandling.Parse(lenD[0]);
            lengthY = TextHandling.Parse(lenD[1]);
        }

        public void UpdateLengthY(int bulean, int valor, int WallOld, int WallHeight)
        {
            // Cada nivel de parede são 55 de altura.
            // possui 16 niveis de parede
            // a altura mais baixa é 31 (padrão)
            // a altura mais alta é 31 + 55 (um nivel) + (Altura nova 0-15 * 64) 64 = ? (possivelmente a area)
            // area total minima = 31 + 55 + (1 * 64) = 150
            // bulean = se a parede antiga for menor que a nova, estamos diminuindo o tamanho dela
            // valor = tamanho da parede antigo - tamanho da parede nova (int)
            // WallHeight = o nivel da parede nova (0-15) (o padrão é 0)
            if (bulean == 3)
            {
                // 22 = 1/4 da altura minima + 1 nivel
                // 64 = 3/4 da altura minima + 1 nivel

                /*if (lengthY > (22 + 64 * (WallHeight + 1)))
                {
                    lengthY -= (64 * (lengthY / 64));

                    if (lengthY < 31)
                        lengthY += 31;
                }*/

                lengthY -= (55 * (WallOld - WallHeight));

            }
            else if(bulean == 1)
            {
                lengthY += (64 * valor);
            }
        }

        public override string ToString()
        {
            return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + side;
        }
    }
}
