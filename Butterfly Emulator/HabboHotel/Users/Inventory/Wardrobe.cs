namespace ButterStorm.HabboHotel.Users.Inventory
{
    class Wardrobe
    {
        internal int slotId;
        internal string look;
        internal string gender;
        internal bool needInsert;

        internal Wardrobe(int slot, string sLook, string sex)
        {
            slotId = slot;
            look = sLook;
            gender = sex;
            needInsert = false;
        }
    }
}
