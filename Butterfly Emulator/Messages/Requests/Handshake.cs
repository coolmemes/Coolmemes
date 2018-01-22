using System;
using ButterStorm;
using HabboEvents;
using Azure.Encryption;
using Azure.Encryption.Hurlant.Crypto.Prng;

namespace Butterfly.Messages
{
    partial class GameClientMessageHandler
    {
        internal void CheckRelease() { }

        internal void InitCrypto()
        {
            Response.Init(Outgoing.SendBannerMessageComposer);
            Response.AppendString(Handler.GetRsaDiffieHellmanPrimeKey());
            Response.AppendString(Handler.GetRsaDiffieHellmanGeneratorKey());
            SendResponse();
        }

        internal void InitSecretKey()
        {
            var cipherKey = Request.PopFixedString();
            var sharedKey = Handler.CalculateDiffieHellmanSharedKey(cipherKey);

            if (sharedKey != 0)
            {
                Response.Init(Outgoing.SecretKeyComposer);
                Response.AppendString(Handler.GetRsaDiffieHellmanPublicKey());
                Response.AppendBoolean(EmuSettings.CRYPTO_CLIENT_SIDE); // enable RC4
                SendResponse();

                var data = sharedKey.ToByteArray();

                if (data[data.Length - 1] == 0)
                    Array.Resize(ref data, data.Length - 1);

                Array.Reverse(data, 0, data.Length);

                Session.GetConnection().ARC4ServerSide = new ARC4(data);
                if (EmuSettings.CRYPTO_CLIENT_SIDE)
                    Session.GetConnection().ARC4ClientSide = new ARC4(data);
            }
        }

        internal void setClientVars()
        {
            string swfs = Request.PopFixedString();
            string vars = Request.PopFixedString();
        }

        internal void setUniqueIDToClient()
        {
            string MachineId = Request.PopFixedString();
            string md5FingerPrint = Request.PopFixedString();

            if (MachineId.Length <= 0)
            {
                MachineId = OtanixEnvironment.GenerateRandomString();

                ServerMessage creatingMachine = new ServerMessage(Outgoing.GenerateMachineId);
                creatingMachine.AppendString(MachineId);
                Session.SendMessage(creatingMachine);
            }

            Session.MachineId = MachineId;
        }

        internal void SSOLogin()
        {
            if (Session.GetHabbo() == null)
                Session.tryLogin(Request.PopFixedString());
            else
                Session.SendNotif("Usuario ya conectado.");
        }
    }
}
