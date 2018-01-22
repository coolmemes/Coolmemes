using ButterStorm;

namespace Butterfly.Messages
{
    partial class GameClientMessageHandler
    {
        public void OpenQuests()
        {
            OtanixEnvironment.GetGame().GetQuestManager().GetList(Session, Request);
        }

        public void StartQuest()
        {
            OtanixEnvironment.GetGame().GetQuestManager().ActivateQuest(Session, Request);
        }

        public void StopQuest()
        {
            OtanixEnvironment.GetGame().GetQuestManager().CancelQuest(Session, Request);
        }

        public void GetCurrentQuest()
        {
            OtanixEnvironment.GetGame().GetQuestManager().GetCurrentQuest(Session, Request);
        }
    }
}
