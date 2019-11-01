==============================================================
=============== Crypto Service Cert Readme ===================
==============================================================
The Dev Application keys are encrypted with the certificate that is in this folder.

Setting up the project
-----------------------------
1. Add the certificate to the trusted certs in your personal certificate store
2. Run the unit tests

Preparing a new environment
1. Generate a new self-signed certificate using PowerShell
   New-SelfSignedCertificate -Type Custom -Subject "CN=InoxCA,OU=UserAccounts,DC=corp,DC=contoso,DC=com" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2","2.5.29.17={text}upn=CryptoService") -KeyUsage DigitalSignature -KeyAlgorithm RSA -KeyLength 2048 -CertStoreLocation "Cert:\CurrentUser\My"

2. Export the certificate with Private Key and install in the Developer or Service account personal cert store under trusted certificats
3. Give the certificate a friendly name of "CryptoServiceCert" (Verify the name in the Settings File
4. Run the unit test to generate a new symettric key for both Red and Blue
5. Encrypt both the Red and Blue keys with the new certificate





