using Butterfly.Core;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.Premiums;
using Butterfly.HabboHotel.Premiums.Catalog;
using Butterfly.HabboHotel.Rooms;
using ButterStorm;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.Messages
{
    partial class GameClientMessageHandler
    {
        public void PlacePremiumItem(bool FloorItem)
        {
            Room CurrentRoom = Session.GetHabbo().CurrentRoom;
            if (CurrentRoom == null)
                return;

            // Si no es el dueño de sala:
            if(CurrentRoom.RoomData.OwnerId != Session.GetHabbo().Id)
            {
                NotificaStaff.Notifica(Session, true);
                return;
            }

            // Si este usuario no es premium:
            if (!Session.GetHabbo().IsPremium())
            {
                NotificaStaff.Notifica(Session);
                return;
            }

            // Si ha llegado al límite de furnis establecidos:
            if (Session.GetHabbo().GetPremiumManager().GetActualItems() >= Session.GetHabbo().GetPremiumManager().GetMaxItems())
            {
                ServerMessage messageError = new ServerMessage(Outgoing.CustomAlert);
                messageError.AppendString("furni_placement_error");
                messageError.AppendInt32(1);
                messageError.AppendString("message");
                messageError.AppendString("${room.error.max_furniture}");
                Session.SendMessage(messageError);

                return;
            }

            // Obtenemos la página del catálogo.
            int PageId = Request.PopWiredInt32();
            CatalogPremiumPage Page = OtanixEnvironment.GetGame().GetCatalogPremium().GetPage(PageId);
            if (Page == null || !Page.Enable || !Page.Visible)
                return;

            // Obtenemos el item del catálogo.
            uint ItemId = Request.PopWiredUInt();
            CatalogPremiumItem Item = Page.GetItem(ItemId);
            if (Item == null)
                return;

            Request.PopFixedString();

            if (FloorItem)
            {
                int X = Request.PopWiredInt32();
                int Y = Request.PopWiredInt32();
                int Rot = Request.PopWiredInt32();

                RoomItem RoomItem = new RoomItem(EmuSettings.PREMIUM_BASEID + Session.GetHabbo().GetPremiumManager().GetValidPosition(), CurrentRoom.RoomId, Item.BaseId, "", CurrentRoom.RoomData.OwnerId, X, Y, 0, Rot, CurrentRoom, true);

                if (CurrentRoom.GetRoomItemHandler().SetFloorItem(Session, RoomItem, X, Y, Rot, true, false, true, false) == false)
                {
                    Session.GetHabbo().GetPremiumManager().ModifyItemPosition((int)(RoomItem.Id - EmuSettings.PREMIUM_BASEID), false);
                    return;
                }
            }
            else
            {
                string W = Request.PopFixedString();

                WallCoordinate coordinate = new WallCoordinate(W);
                RoomItem RoomItem = new RoomItem(EmuSettings.PREMIUM_BASEID + Session.GetHabbo().GetPremiumManager().GetValidPosition(), CurrentRoom.RoomId, Item.BaseId, "", CurrentRoom.RoomData.OwnerId, coordinate, CurrentRoom, true);

                if (CurrentRoom.GetRoomItemHandler().SetWallItem(Session, RoomItem) == false)
                {
                    Session.GetHabbo().GetPremiumManager().ModifyItemPosition((int)(RoomItem.Id - EmuSettings.PREMIUM_BASEID), false);
                    return;
                }
            }

            // Incrementamos en 1 los items usados.
            Session.GetHabbo().GetPremiumManager().IncreaseItems();

            // Actualizamos el packet del catálogo.
            Session.SendMessage(PremiumManager.SerializePremiumItemsCount(Session.GetHabbo()));
        }

        public void PurchasableClothingConfirmation()
        {
            Room CurrentRoom = Session.GetHabbo().CurrentRoom;
            if (CurrentRoom == null || !CurrentRoom.CheckRights(Session, true))
                return;

            RoomItem Item = CurrentRoom.GetRoomItemHandler().GetItem(Request.PopWiredUInt());
            if (Item == null || Item.GetBaseItem() == null)
                return;

            // Si esta ropa ya la tiene el usuario.
            if (Session.GetHabbo().GetUserClothingManager().ContainsClothes(Item.GetBaseItem().Name))
                return;

            // Eliminamos al item de la sala.
            CurrentRoom.GetRoomItemHandler().RemoveFurniture(Session, Item);

            // Añadimos la ropa a la caché y a la DB.
            Session.GetHabbo().GetUserClothingManager().AddClothesToSQL(Item.GetBaseItem().Name);

            // Actualizamos estados.
            Session.SendMessage(Session.GetHabbo().GetUserClothingManager().SerializeClothes());

            // Alerta
            Session.GetMessageHandler().GetResponse().Init(Outgoing.CustomAlert);
            Session.GetMessageHandler().GetResponse().AppendString("figureset.redeemed.success");
            Session.GetMessageHandler().GetResponse().AppendInt32(0);
            Session.GetMessageHandler().SendResponse();
        }
    }
}
