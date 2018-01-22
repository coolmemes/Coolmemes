using SharedPacketLib;

namespace Butterfly.Net
{
    public class InitialPacketParser : IDataParser
    {
        public delegate void NoParamDelegate();
        public delegate void DualparamDelegate(byte[] data);
        public event NoParamDelegate PolicyRequest;
        public event DualparamDelegate SwitchParserRequest;

        public void handlePacketData(byte[] packet)
        {
            if (packet[0] == 60 && PolicyRequest != null)
            {
                PolicyRequest.Invoke();
            }
            else if(packet[0] != 67  && SwitchParserRequest != null)
            {
                SwitchParserRequest.Invoke(packet);
            }
        }

        public void Dispose()
        {
            PolicyRequest = null;
            SwitchParserRequest = null;
        }

        public object Clone()
        {
            return new InitialPacketParser();
        }
    }
}
