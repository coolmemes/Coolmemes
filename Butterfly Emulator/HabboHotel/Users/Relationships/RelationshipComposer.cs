using System;
using System.Collections.Generic;
using Butterfly.Core;
using System.Collections;
namespace Butterfly.HabboHotel.Users.Relationships
{
    class RelationshipComposer
    {
        internal Dictionary<uint, Relationship> LoveRelation;
        internal Dictionary<uint, Relationship> FriendRelation;
        internal Dictionary<uint, Relationship> DieRelation;

        internal RelationshipComposer(List<Relationship> relations)
        {
            LoveRelation = new Dictionary<uint, Relationship>();
            FriendRelation = new Dictionary<uint, Relationship>();
            DieRelation = new Dictionary<uint, Relationship>();

            foreach (var rel in relations)
            {
                try
                {
                    if (rel.RelationType == 1 && !LoveRelation.ContainsKey(rel.MemberId))
                        LoveRelation.Add(rel.MemberId, rel);
                    else if (rel.RelationType == 2 && !FriendRelation.ContainsKey(rel.MemberId))
                        FriendRelation.Add(rel.MemberId, rel);
                    else if (rel.RelationType == 3 && !DieRelation.ContainsKey(rel.MemberId))
                        DieRelation.Add(rel.MemberId, rel);
                }
                catch (Exception e)
                {
                    Logging.LogException("Relationship >> " + e);
                }
            }
        }

        internal void Destroy()
        {
            LoveRelation.Clear();
            FriendRelation.Clear();
            DieRelation.Clear();

            LoveRelation = null;
            FriendRelation = null;
            DieRelation = null;
        }
    }
}
