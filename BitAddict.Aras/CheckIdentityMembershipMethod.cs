using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aras.IOM;

namespace BitAddict.Aras
{
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

            var ids = new List<string> {userAlias.getProperty("related_id")};

            // not the fastest, but it works.
            // should be optimized by asking for batches of identities.
            // (see below for fast recursive SQL .. that Aras doesn't permit in ApplySQL.)
            while (ids.Any())
            {
                var id = ids.Last();
                ids.RemoveAt(ids.Count - 1);

                var identityItem = Innovator.newItem("Identity", "get");
                identityItem.setAttribute("select", "keyed_name");

                var memberRelation = identityItem.createRelationship("Member", "get");
                memberRelation.setAttribute("select", "keyed_name");
                memberRelation.setProperty("related_id", id);
                identityItem = Innovator.ApplyItem(identityItem);

                if (identityItem.Enumerate()
                    .Any(i => i.getProperty("keyed_name") == IdentityName))
                    return Innovator.newResult("true");

                ids.AddRange(identityItem.Enumerate().Select(i => i.getID()));
            }

            return Innovator.newResult("false");
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
}
