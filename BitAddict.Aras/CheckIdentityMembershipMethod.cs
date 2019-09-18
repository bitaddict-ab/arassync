// MIT License, see COPYING.TXT

using System;
using System.Collections.Generic;
using System.Linq;
using Aras.IOM;

namespace BitAddict.Aras
{
    /// <inheritdoc />
    /// <summary>
    /// Check if user is an (possibly indirect) member of an identity group
    /// </summary>
    public class CheckIdentityMembershipMethod : ArasMethod
    {
        /// <summary>
        /// Name of Identity to check
        /// </summary>
        [XmlProperty("identity_name", true)]
        public string IdentityName { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Check if user is an (possibly indirect) member of an identity group
        /// </summary>
        /// <param name="body">Method XML body item</param>
        /// <returns>'true' as string result if membership matches</returns>
        public override Item DoApply(Item body)
        {
            XmlPropertyAttribute.BindXml(body.node, this);

            var userId = Innovator.getUserID();

            var userAlias = Innovator.newItem("Alias", "get");
            userAlias.setAttribute("select", "related_id");
            userAlias.setProperty("source_id", userId);
            userAlias = Innovator.ApplyItem(userAlias);

            var identityId = userAlias.getProperty("related_id");
            return Innovator.newResult(CheckIfMemberOfIdentity(identityId) ? "true" : "false");
        }

        /// <summary>
        /// Check if an identity ID is a member of the identity with name `IdentityName`.
        ///
        /// Also takes into account the from and end dates for an identity membership.
        /// </summary>
        /// <param name="identityId">The identity ID to check if it is a member of identity with name `IdentityName`.</param>
        private bool CheckIfMemberOfIdentity(string identityId)
        {
            var identityIds = new List<Tuple<string, DateTime, DateTime>>
                {new Tuple<string, DateTime, DateTime>(identityId, DateTime.MinValue, DateTime.MaxValue)};

            while (identityIds.Any())
            {
                var identityIdTuple = identityIds.Last();
                identityIds.RemoveAt(identityIds.Count - 1);
                identityId = identityIdTuple.Item1;
                var fromDate = identityIdTuple.Item2;
                var endDate = identityIdTuple.Item3;

                var identityItems = Innovator.newItem("Identity", "get");
                identityItems.setAttribute("select", "keyed_name");

                var memberRelation = identityItems.createRelationship("Member", "get");
                memberRelation.setAttribute("select", "id, from_date, end_date");
                memberRelation.setProperty("related_id", identityId);
                identityItems = Innovator.ApplyItem(identityItems);

                foreach (var identityItem in identityItems.Enumerate())
                {
                    var memberItem = identityItem.getRelationships().getItemByIndex(0);
                    var newFromDate = MaxDateTime(fromDate, memberItem.getProperty("from_date")?.ToDateTime());
                    var newEndDate = MinDateTime(endDate, memberItem.getProperty("end_date")?.ToDateTime());

                    if (identityItem.getProperty("keyed_name") == IdentityName && DateTime.Now >= newFromDate &&
                        DateTime.Now <= newEndDate)
                        return true;

                    identityIds.Add(
                        new Tuple<string, DateTime, DateTime>(identityItem.getID(), newFromDate, newEndDate));
                }
            }

            return false;
        }

        private static DateTime MinDateTime(DateTime d1, DateTime? d2)
        {
            if (d2 == null) return d1;
            return d1 < d2.Value ? d1 : d2.Value;
        }

        private static DateTime MaxDateTime(DateTime d1, DateTime? d2)
        {
            if (d2 == null) return d1;
            return d1 > d2.Value ? d1 : d2.Value;
        }

        /* Aras returns no result for the recursive query below. :(
         * Maybe it rejects SQL as query can't start with comment either. X-(
         
        Workaround: Add as stored procedure via SQL Item table and call as follows:
         
        Item proc = this.newItem("SQL", "SQL PROCESS");
        proc.setProperty("name", "CheckIdentityMembership");
        proc.setProperty("PROCESS", "CALL");
        proc.setProperty("ARG1", Innovator.getUserID()); 
        proc.setProperty("ARG2", IdentityName);
        Item result = proc.apply(); 

        SQL body:

        CREATE PROCEDURE innovator.CheckIdentityMembership
            @userId nvarchar(128),
            @identityName nvarchar(128)
        AS
        BEGIN

        WITH IdentityMembers (keyed_name, parent_name, id, [level])
            AS
            (
                -- anchor definition
                SELECT i.keyed_name, CONVERT(NVARCHAR(128),'') AS parent_name,
                        i.id, 0 as [level]
                FROM [innovator].[IDENTITY] i
                INNER JOIN [innovator].[ALIAS] a ON a.related_id = i.id
                WHERE a.source_id = @userId
                -- recursive definition
                UNION ALL
                SELECT i.keyed_name, im.keyed_name AS parent_name, 
                        i.id, im.[level] + 1 as [level]
                FROM [innovator].[MEMBER] m
                INNER JOIN IdentityMembers im ON im.id = m.related_id
                INNER JOIN [innovator].[IDENTITY] i ON i.id = m.source_id
            )
            SELECT 
                -- get some extra data for debugging
                keyed_name, parent_name, id, [level] 
                -- faster version
                --COUNT(1) 
            FROM IdentityMembers
            WHERE keyed_name = @identityName
       END
        */
    }

    internal static class CheckIdentityMembershipMethodExtensions
    {
        public static DateTime? ToDateTime(this string str)
        {
            return DateTime.Parse(str);
        }
    }
}
