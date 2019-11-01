using System;
using System.Collections.Generic;

namespace Spike.Cryptography.Tests.Models
{
    public class FabricSigning
    {
        public Guid RootWorkflowInstanceId { get; set; }

        public string Title { get; set; }

        public string ClientId { get; set; }

        public string SubjectNoxId { get; set; }

        public IEnumerable<SignedFile> Files { get; set; }

        public IEnumerable<SignedContent> Content { get; set; }

        public IEnumerable<SignedAgreement> Agreements { get; set; }
    }
}
