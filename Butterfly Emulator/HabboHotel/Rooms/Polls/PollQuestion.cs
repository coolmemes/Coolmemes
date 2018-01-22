using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Polls
{
    class PollQuestion
    {
        internal uint Id;
        internal string QuestionTitle;
        internal List<string> Answers;
        internal uint PollType;

        internal PollQuestion(string question)
        {
            this.Answers = new List<string>();

            this.Id = Convert.ToUInt32(question.Split(';')[0]);
            this.QuestionTitle = question.Split(';')[1];

            for (int i = 2; i < question.Split(';').Length - 1; i++)
            {
                this.Answers.Add(question.Split(';')[i]);
            }

            this.PollType = Convert.ToUInt32(question.Split(';')[question.Split(';').Length - 1]);
        }
    }
}
