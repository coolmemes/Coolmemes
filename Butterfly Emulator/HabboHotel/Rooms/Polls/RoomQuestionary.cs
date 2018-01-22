using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Polls
{
    class RoomQuestionary : IRoomPolls
    {
        private uint RoomId;
        private string Description;
        private PollQuestion[] Questions;
        private string LastMessage;

        /// <summary>
        /// Identifica el tipo de cuestionario.
        /// </summary>
        /// <returns>PollType</returns>
        public PollType GetPollType()
        {
            return PollType.ROOM_QUESTIONARY;
        }

        /// <summary>
        /// Retorna el Id de Sala en la que se encontraba la encuesta.
        /// </summary>
        /// <returns></returns>
        public uint GetRoomId()
        {
            return this.RoomId;
        }

        /// <summary>
        /// Retorna la pregunta inicial que se muestra al abrir la encuesta.
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            return this.Description;
        }

        /// <summary>
        /// Retorna las cuestiones que se realizarán durante la encuesta.
        /// </summary>
        /// <returns></returns>
        public PollQuestion[] GetQuestions()
        {
            return this.Questions;
        }

        /// <summary>
        /// Retorna el último mensaje que se mostrará al finalizar la encuesta.
        /// </summary>
        /// <returns></returns>
        public string GetLastMessage()
        {
            return this.LastMessage;
        }

        /// <summary>
        /// Comprueba si existe una encuesta y si existe la carga.
        /// </summary>
        /// <param name="RoomId"></param>
        public bool LoadQuestionary(uint RoomId)
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM room_polls WHERE room_id = '" + RoomId + "'");
                DataRow dRow = dbClient.getRow();
                if (dRow == null)
                    return false; // no poll in room.

                string[] arrayQuestions = ((string)dRow["questions"]).Replace("\\\\", "\\").Split('\n');

                this.Questions = new PollQuestion[arrayQuestions.Length];
                this.RoomId = Convert.ToUInt32(dRow["room_id"]);
                this.Description = (string)dRow["main_info"];
                for (int i = 0; i < arrayQuestions.Length; i++)
                {
                    this.Questions[i] = new PollQuestion(arrayQuestions[i]);
                }
                this.LastMessage = (string)dRow["end_message"];
            }
            return true;
        }

        /// <summary>
        /// Monta el paquete que mostrará la encuesta al usuario.
        /// </summary>
        /// <returns></returns>
        public ServerMessage SerializePoll()
        {
            ServerMessage Message = new ServerMessage(Outgoing.PollContentsParser);
            Message.AppendUInt(this.RoomId);
            Message.AppendString(this.Description);
            Message.AppendString(this.LastMessage);
            Message.AppendInt32(this.Questions.Length);
            foreach (PollQuestion pollQuestion in this.Questions)
            {
                Message.AppendUInt(pollQuestion.Id); // Id
                Message.AppendInt32(0); // value i++ ??
                Message.AppendUInt(pollQuestion.PollType); // 1, 2, 3 -> 1 (raddiobtn), 2 (checkbox), 3 (richtextbox)
                Message.AppendString(pollQuestion.QuestionTitle);
                Message.AppendInt32(1); // Selection Min
                Message.AppendUInt(pollQuestion.PollType); // 1, 2, 3 -> 1 (raddiobtn), 2 (checkbox), 3 (richtextbox)
                Message.AppendInt32(pollQuestion.Answers.Count); // selection Max

                if (pollQuestion.PollType == 1 || pollQuestion.PollType == 2)
                {
                    for (int ii = 1; ii <= pollQuestion.Answers.Count; ii++)
                    {
                        Message.AppendString(ii.ToString());
                        Message.AppendString(pollQuestion.Answers[ii - 1]);
                        Message.AppendInt32(1); // ??
                    }
                }

                Message.AppendInt32(0);
            }

            Message.AppendBoolean(true);
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
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO room_polls_answers VALUES ('" + UserId + "','" + RoomId + "','" + 0 + "',@answers)");
                dbClient.addParameter("answers", Answers);
                dbClient.runQuery();
            }

            return null;
        }

        public ServerMessage ClearInformation()
        {
            return null;
        }
    }
}
