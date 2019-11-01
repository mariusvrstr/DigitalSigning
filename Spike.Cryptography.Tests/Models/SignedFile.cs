using System;

namespace Spike.Cryptography.Tests.Models
{
    public class SignedFile
    {
        public Guid FileId { get; set; }

        public string FileUrl { get; set; }

        public string FileName { get; set; }

        public string FileHash { get; set; }
    }
}
