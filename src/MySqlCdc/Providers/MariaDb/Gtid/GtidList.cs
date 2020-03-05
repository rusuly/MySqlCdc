using System;
using System.Collections.Generic;
using System.Linq;
using MySqlCdc.Events;

namespace MySqlCdc.Providers.MariaDb
{
    /// <summary>
    /// Represents GtidList from MariaDB.
    /// </summary>
    public class GtidList : IGtidState
    {
        private const string NotSupportedMessage = "Currently, BinlogClient doesn't support MariaDB multi-master setup";

        /// <summary>
        /// Gets a list of Gtids per each domain.
        /// </summary>
        public List<Gtid> Gtids { get; }

        /// <summary>
        /// Parses <see cref="GtidList"/> from string representation.
        /// </summary>
        public static GtidList Parse(string gtidList)
        {
            if (gtidList == null)
                throw new ArgumentNullException(nameof(gtidList));

            if (gtidList == "")
                return new GtidList();

            var gtids = gtidList.Replace("\n", "")
                .Split(',')
                .Select(x => x.Trim())
                .ToArray();

            if (gtids.Length > 1)
                throw new NotSupportedException(NotSupportedMessage);

            var result = new GtidList();
            foreach (var gtid in gtids)
            {
                string[] components = gtid.Split('-');
                long domainId = long.Parse(components[0]);
                long serverId = long.Parse(components[1]);
                long sequence = long.Parse(components[2]);
                result.Gtids.Add(new Gtid(domainId, serverId, sequence));
            }
            return result;
        }

        /// <summary>
        /// Adds a gtid value to the GtidList.
        /// </summary>
        public bool AddGtid(IGtid gtidRaw)
        {
            var gtid = (Gtid)gtidRaw;
            if (!Gtids.Any())
            {
                Gtids.Add(gtid);
                return true;
            }
            else if (Gtids[0].DomainId == gtid.DomainId)
            {
                Gtids[0] = gtid;
                return true;
            }
            else throw new NotSupportedException(NotSupportedMessage);
        }

        /// <summary>
        /// Constructs string representation of the GtidList.
        /// </summary>
        public string GetSlaveConnectState()
        {
            if (!Gtids.Any())
                return string.Empty;

            return Gtids[0].ToString();
        }
    }
}
