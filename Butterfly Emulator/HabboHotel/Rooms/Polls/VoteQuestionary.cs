using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Polls
{
    class VoteQuestionary : IRoomPolls
    {
        private uint RoomId;
        private string Question;
        private uint Time;
        private List<uint> PositiveVote;
        private List<uint> NegativeVote;

        /// <summary>
        /// Identifica el tipo de cuestionario.
        /// </summary>
        /// <returns>PollType</returns>
        public PollType GetPollType()
        {
            return PollType.VOTE_QUESTIONARY;
        }

        /// <summary>
        /// Crea la votación.
        /// </summary>
        /// <param name="RoomId">Id de sala</param>
        /// <returns></returns>
        public bool LoadQuestionary(uint RoomId)
        {
            this.RoomId = RoomId;
            return true;
        }

        /// <summary>
        /// Monta el paquete que mostrará la encuesta al usuario.
        /// </summary>
        /// <returns></returns>
        public ServerMessage SerializePoll()
        {
            ServerMessage Message = new ServerMessage(Outgoing.QuestionParser);
            Message.AppendString("MATCHING_POLL");
            Message.AppendInt32(0); // Main ID
            Message.AppendInt32(0); // Second ID
            Message.AppendUInt(this.Time * 1000); // Duration
            Message.AppendInt32(0); // [Second ID]
            Message.AppendUInt(0); // [Number]
            Message.AppendUInt(6); // [Type]
            Message.AppendString(this.Question); // [Content]
            // if (Type == 1 || Type == 2) // desactivado por el momento
            return Message;
        }

        /// <summary>
        /// Acción que reliaza cuando un usuario termina la encuesta.
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Answers"></param>
        /// <returns></returns>
        public ServerMessage SaveInformation(uint UserId, string Answers)
        {
            if (Answers.Equals("1"))
                this.PositiveVote.Add(UserId);
            else if (Answers.Equals("0"))
                this.NegativeVote.Add(UserId);

            ServerMessage Message = new ServerMessage(Outgoing.QuestionResponseParser);
            Message.AppendUInt(UserId); // UserId
            Message.AppendString(Answers); // Value
            Message.AppendUInt(2); // Votaciones Positivas y Negativas
            Message.AppendString("0"); // Negativas
            Message.AppendInt32(this.NegativeVote.Count); // Número de negativas
            Message.AppendString("1"); // Positivas
            Message.AppendInt32(this.PositiveVote.Count); // Número de positivas
            return Message;
        }

        public ServerMessage ClearInformation()
        {
            ServerMessage Message = new ServerMessage(Outgoing.QuestionClean);
            Message.AppendUInt(0); // PollId
            Message.AppendInt32(2); // Foreach
            Message.AppendString("0"); // Negativas
            Message.AppendInt32(this.NegativeVote.Count); // Número de negativas
            Message.AppendString("1"); // Positivas
            Message.AppendInt32(this.PositiveVote.Count); // Número de positivas
            return Message;
        }

        /// <summary>
        /// Cargamos la información de la votación.
        /// </summary>
        /// <param name="Question"></param>
        /// <param name="Time"></param>
        public void LoadInformation(string Question, uint Time)
        {
            this.Question = Question;
            this.Time = Time;
            this.PositiveVote = new List<uint>();
            this.NegativeVote = new List<uint>();
        }

        /// <summary>
        /// Comprueba si un usuario ya ha votado o no.
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public bool UserVote(uint UserId)
        {
            return this.PositiveVote.Contains(UserId) || this.NegativeVote.Contains(UserId);
        }
    }
}
