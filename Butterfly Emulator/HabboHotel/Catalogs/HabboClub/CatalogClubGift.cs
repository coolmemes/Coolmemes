using Butterfly.HabboHotel.Items;

namespace Butterfly.Catalogs.HabboClub
{
    public class CatalogClubGift
    {
        internal int Id { get; set; }
        internal int BaseId { get; set; }
        internal int DaysRequired { get; set; }
        internal Item Item { get; set; }

        internal CatalogClubGift(int Id, int BaseId, int DaysRequired, Item Item)
        {
            this.Id = Id;
            this.BaseId = BaseId;
            this.DaysRequired = DaysRequired;
            this.Item = Item;
        }      
    }
}
