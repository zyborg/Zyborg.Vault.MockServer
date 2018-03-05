using System;

namespace Zyborg.Vault.MockServer.Authentication
{
    public class Token
    {
        private static readonly string[] EmptyStringsValue = new string[0];

        public string Id { get; set; }

        public string Accessor { get; set; }

        public DateTime? IssueTime { get; set; }

        public DateTime CreationTime { get; set; } = DateTime.UtcNow;

        public long CreationTtl { get; set; }

        public long Ttl { get; set; }

        public DateTime ExpireTime { get; set; }

        public long ExplicitMaxTtl { get; set; }

        public string Path { get; set; }

        public string DisplayName { get; set; }

        public bool Orphan { get; set; }

        public bool Renewable { get; set; }

        public int NumUses { get; set; }

        public string[] Policies { get; set; } = EmptyStringsValue;

        public object Meta { get; set; }
    }
}