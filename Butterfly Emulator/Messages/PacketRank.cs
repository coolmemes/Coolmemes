namespace Butterfly.Messages
{
    struct FusedPacket
    {
        internal readonly ServerMessage content;
        internal readonly string requirements;
        internal readonly uint userId;

        public FusedPacket(ServerMessage content, string requirements, uint userId)
        {
            this.content = content;
            this.requirements = requirements;
            this.userId = userId;
        }
    }
}
