using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Spike.Cryptography.Tests.Models;
using Spike.CryptographyService.Cryptography;
using Spike.CryptographyService.DAL;
using Spike.CryptographyService.Models.DigitalSigning;
using Spike.CryptographyService.Repository;
using Spike.CryptographyService.Simulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spike.Seedworks.CryptoService;

namespace Spike.Cryptography.Tests
{
    [TestClass]
    public class Tests
    {
        private static void RegisterChilKat()
        {
            var glob = new Chilkat.Global();
            var success = glob.UnlockBundle("Anything for 30-day trial");
        }

        private FabricSigning GenerateSampleFabricSignContent()
        {
            return new FabricSigning
            {
                Title = "Terms & Conditions",
                ClientId = "A00123",
                SubjectNoxId = "NX23245",

                Content = new List<SignedContent>
                {
                    new SignedContent
                    {
                        Name = "Terms and Conditions",
                        ContentHash = "#112233"
                    }
                },
                Agreements = new List<SignedAgreement>
                {
                    new SignedAgreement
                    {
                        Description = "I agree to the terms and conditions"
                    }
                },
                Files = new List<SignedFile>
                {
                    new SignedFile
                    {
                        FileId = Guid.NewGuid(),
                        FileName = "Conflict of interest policy",
                        FileHash = "#12345",
                        FileUrl = @"https://filevault.com/#12345"
                    }
                }
            };
        }

        [TestMethod]
        public void EncryptAndDecryptContentTest()
        {
            var encryptionManager = new EncryptionManager(new KeyStore(new KeyStoreRepository(new DataContext())));
            var hashManager = new HashManager();

            var cryptoService = new CryptographyService.CryptographyService(encryptionManager, hashManager);
            var wrappedCryptoService = new CryptoServiceWrapper<string>(cryptoService, null);

            const string myContent = "Sample text to encrypt";
            var salt = Guid.NewGuid();

            var encryptedOutcome = wrappedCryptoService.Encrypt(myContent, salt.ToString());
            var decryptedOutcome = wrappedCryptoService.Decrypt(encryptedOutcome, salt.ToString());

            Assert.IsTrue(myContent == decryptedOutcome);
        }

        [TestMethod]
        public void TestBasicSigningAndVerification()
        {
            var encryptionManager = new EncryptionManager(new KeyStore(new KeyStoreRepository(new DataContext())));
            var hashManager = new HashManager();

            var cryptoService = new CryptographyService.CryptographyService(encryptionManager, hashManager);
            var wrappedCryptoService = new CryptoServiceWrapper<FabricSigning>(cryptoService, null);

            var signingContent = GenerateSampleFabricSignContent();
            var signingOutcome = wrappedCryptoService.SignContent(signingContent);

            var signature = new DigitalSignature<FabricSigning>()
            {
                OriginalContent = signingContent,
                SignedContent = signingOutcome.Signature,
                SignatoryReference = signingOutcome.SignatoryReference
            };

            var signOutcome = wrappedCryptoService.VerifySignature(signature);

            Assert.IsTrue(signOutcome.SignatoryMatchedToSignature);
            Assert.IsTrue(signOutcome.SignedContentMatchesToSignature);
            
            // This represents application side verification of signed content. Helpful to identify what in content has changed
            Assert.IsTrue(signOutcome.ExpectedContent?.Agreements?.FirstOrDefault().Description == signOutcome.SignedContent?.Agreements?.FirstOrDefault().Description);

        }

        [TestMethod]
        public void TestSimulatedBasicSigningAndVerification()
        {
            var cryptoService = new CryptographySimulatedService();
            var wrappedCryptoService = new CryptoServiceWrapper<FabricSigning>(cryptoService, null);

            var signingContent = GenerateSampleFabricSignContent();
            var signingOutcome = wrappedCryptoService.SignContent(signingContent);

            var signature = new DigitalSignature<FabricSigning>()
            {
                OriginalContent = signingContent,
                SignedContent = signingOutcome.Signature,
                SignatoryReference = signingOutcome.SignatoryReference
            };

            var signOutcome = wrappedCryptoService.VerifySignature(signature);

            Assert.IsTrue(signOutcome.SignatoryMatchedToSignature);
            Assert.IsTrue(signOutcome.SignedContentMatchesToSignature);

            // This represents application side verification of signed content. Helpful to identify what in content has changed
            Assert.IsTrue(signOutcome.ExpectedContent?.Agreements?.FirstOrDefault().Description == signOutcome.SignedContent?.Agreements?.FirstOrDefault().Description);
        }

        [TestMethod]
        [Ignore]
        public void GenerateSymetricEncryptionKey()
        {
            var symetricKey = EncryptionManager.GenerateSymmetricKey();
            Console.WriteLine($"Symmetric Key: [{symetricKey}]");

            Assert.IsNotNull(symetricKey);
        }

        [TestMethod]
        [Ignore]
        public void EncryptApplicationKeyFromCryptoServiceCert()
        {
            RegisterChilKat();
            X509Certificate2 theCert = null;

            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            store.Open(OpenFlags.ReadOnly);

            foreach (var certificate in store.Certificates)
            {
                //TODO's
                Console.WriteLine(certificate.FriendlyName);

                if (certificate.FriendlyName == "CryptoServiceCert")
                {
                    theCert = certificate;
                }
            }

            var privateKey = theCert?.PrivateKey?.ToXmlString(true);
            var publicKey = theCert?.PrivateKey?.ToXmlString(true);

            var sampleText = "+oPDU29Bv2hfUZ8fZozMGAVYUZjp1wdwDs42JY213tA=";

            var rsaEncryptor = new Chilkat.Rsa { EncodingMode = "hex" };
            rsaEncryptor.ImportPublicKey(publicKey);
            var encryptedText = rsaEncryptor.EncryptStringENC(sampleText, false);
            Console.WriteLine($"Encrypted Value: [{encryptedText}]");

            var rsaDecryptor = new Chilkat.Rsa { EncodingMode = "hex" };
            rsaDecryptor.ImportPrivateKey(privateKey);

            var decryptedText = rsaDecryptor.DecryptStringENC(encryptedText, true);
        }
    }
}
