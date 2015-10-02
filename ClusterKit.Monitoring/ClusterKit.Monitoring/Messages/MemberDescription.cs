using System.Collections.Generic;
using System.Linq;

namespace ClusterKit.Monitoring.Messages
{
    using Akka.Cluster;

    /// <summary>
    /// Cluster member description
    /// </summary>
    public class MemberDescription
    {
        /// <summary>
        /// status of node
        /// </summary>
        private MemberStatus status;

        public MemberDescription()
        {
        }

        public MemberDescription(Member member)
        {
            this.Address = $"{member.Address.Host}:{member.Address.Port}";
            this.Roles = member.Roles.ToList();
            this.Status = member.Status;
            this.Uid = member.UniqueAddress.Uid;
            this.RoleLeader = new List<string>();
        }

        /// <summary>
        /// Gets or sets address of current node
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this node is cluster leader
        /// </summary>
        public bool IsGlobalLeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this node is currently reachable by cluster
        /// </summary>
        public bool IsReachable { get; set; }

        /// <summary>
        /// Gets or sets last measured ping to node in milliseconds
        /// </summary>
        public double? PingValue { get; set; }

        /// <summary>
        /// Gets or sets list of cluster roles, where this node is a leader
        /// </summary>
        public List<string> RoleLeader { get; }

        /// <summary>
        /// Gets or sets list of roles of this node
        /// </summary>
        public List<string> Roles { get; set; }

        /// <summary>
        /// Gets or sets status of node
        /// </summary>
        public MemberStatus Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = value;
                switch (this.status)
                {
                    case MemberStatus.Up:
                        this.IsReachable = true;
                        break;

                    default:
                        this.IsReachable = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets status name of node
        /// </summary>
        public string StatusName => this.Status.ToString();

        /// <summary>
        /// Gets or sets unique identification of current node incarnation
        /// </summary>
        public int Uid { get; set; }
    }
}