using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButterStorm.HabboHotel.Catalogs
{
    class FreeItemsPatch
    {
        private static uint OFFER_INCREMENTOR = 1;
        private static uint OFFERS_EVERY_X = 6;

        internal static uint GetFreeItems(bool allowOffers, uint creditsPerAmount, uint Amount)
        {
            uint AmountDiscount = 0;
            if (allowOffers)
            {
                AmountDiscount = getLogicalIncrement(Amount);
                AmountDiscount = AmountDiscount + applyIncrementor(Amount);
                AmountDiscount = AmountDiscount + applyExceptions(Amount);
                return (uint)(creditsPerAmount * (Amount - AmountDiscount));
            }
            return (uint)(creditsPerAmount * Amount);
        }

        private static uint getLogicalIncrement(uint amount)
        {
            uint logical = amount / OFFERS_EVERY_X;
            return logical * OFFER_INCREMENTOR;
        }

        private static uint applyIncrementor(uint amount)
        {
            uint x = 0;
            uint i = 0;
            uint logical = amount / OFFERS_EVERY_X;
            if (logical >= OFFER_INCREMENTOR)
            {
                x = amount % OFFERS_EVERY_X;
                if (x == (OFFERS_EVERY_X - 1))
                {
                    i++;
                }
                i = i + (logical - OFFER_INCREMENTOR);
            }
            return i;
        }

        private static uint applyExceptions(uint amount)
        {
            uint i = 0;
            if (amount >= 40)
                i++;
            if (amount >= 99)
                i++;
            return i;
        }
    }
}
