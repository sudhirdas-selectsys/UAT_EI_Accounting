using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.OpenSsl;

namespace Accounting.Utilities
{
    public static class PemUtils
    {
        public static RSA CreateRsaProviderFromPrivateKey(string privateKeyPem)
        {
            using (var reader = new StringReader(privateKeyPem))
            {
                var pemReader = new PemReader(reader);
                var keyObject = pemReader.ReadObject();

                if (keyObject is AsymmetricCipherKeyPair keyPair)
                {
                    var privateKeyParams = (RsaPrivateCrtKeyParameters)keyPair.Private;
                    return CreateRsaFromPrivateKey(privateKeyParams);
                }

                if (keyObject is RsaPrivateCrtKeyParameters privateKey)
                {
                    return CreateRsaFromPrivateKey(privateKey);
                }

                throw new InvalidOperationException("Unsupported private key format.");
            }
        }

        public static RSA CreateRsaProviderFromPublicKey(string publicKeyPem)
        {
            using (var reader = new StringReader(publicKeyPem))
            {
                var pemReader = new PemReader(reader);
                var keyObject = pemReader.ReadObject();

                if (keyObject is RsaKeyParameters pubKey)
                {
                    return CreateRsaFromPublicKey(pubKey);
                }

                // sometimes public key is SubjectPublicKeyInfo structure wrapped as AsymmetricKeyParameter
                if (keyObject is SubjectPublicKeyInfo spki)
                {
                    var pub = (RsaKeyParameters)PublicKeyFactory.CreateKey(spki);
                    return CreateRsaFromPublicKey(pub);
                }

                throw new InvalidOperationException("Unsupported public key format.");
            }
        }

        private static RSA CreateRsaFromPrivateKey(RsaPrivateCrtKeyParameters privKey)
        {
            var rsaParams = DotNetUtilities.ToRSAParameters(privKey);
            var rsa = RSA.Create();
            rsa.ImportParameters(rsaParams);
            return rsa;
        }

        private static RSA CreateRsaFromPublicKey(RsaKeyParameters pubKey)
        {
            var rsaParams = DotNetUtilities.ToRSAParameters(pubKey);
            var rsa = RSA.Create();
            rsa.ImportParameters(rsaParams);
            return rsa;
        }

        // Convenience: read file
        public static RSA CreateRsaFromPrivateKeyFile(string path)
            => CreateRsaProviderFromPrivateKey(File.ReadAllText(path));

        public static RSA CreateRsaFromPublicKeyFile(string path)
            => CreateRsaProviderFromPublicKey(File.ReadAllText(path));
    }
}
