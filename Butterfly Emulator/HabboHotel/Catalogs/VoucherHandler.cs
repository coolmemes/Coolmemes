using System;
using System.Data;
using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;
using ButterStorm;
using HabboEvents;

namespace Butterfly.HabboHotel.Catalogs
{
    class VoucherHandler
    {
        /// <summary>
        /// Check if this Voucher exists. If exists return the prize, else return 0.
        /// </summary>
        /// <param name="Code">Voucher Value</param>
        /// <returns></returns>
        internal static uint GetVoucherValue(string Code)
        {
            DataRow Data;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT value FROM credit_vouchers WHERE code = @code");
                dbClient.addParameter("code", Code);

                Data = dbClient.getRow();
            }

            if (Data != null)
            {
                return Convert.ToUInt32(Data[0]);
            }

            return 0;
        }

        /// <summary>
        /// Delete this Voucher on Database.
        /// </summary>
        /// <param name="Code">Voucher Value</param>
        private static void TryDeleteVoucher(string Code)
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("DELETE FROM credit_vouchers WHERE code = @code");
                dbClient.addParameter("code", Code);
                dbClient.runQuery();
            }
        }

        /// <summary>
        /// Try to insert the prize with a voucher.
        /// </summary>
        /// <param name="Session">User session</param>
        /// <param name="Code">Voucher Value</param>
        internal static void TryRedeemVoucher(GameClient Session, string Code)
        {
            uint Value = GetVoucherValue(Code);

            if (Value == 0)
            {
                ServerMessage Error = new ServerMessage(Outgoing.InvalidVoucher);
                Error.AppendString("1"); // texts: ${catalog.alert.voucherredeem.error.description.X
                Session.SendMessage(Error);

                return;
            }

            TryDeleteVoucher(Code);

            Session.GetHabbo().Diamonds += Value;
            Session.GetHabbo().UpdateExtraMoneyBalance();

            var message = new ServerMessage(Outgoing.CorrectVoucher);
            message.AppendString(""); // productName
            message.AppendString(Value + " diamantes."); // productDescription
            Session.SendMessage(message);
        }
    }
}
