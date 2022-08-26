/*
This file is part of the iText (R) project.
Copyright (c) 1998-2022 iText Group NV
Authors: iText Software.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License version 3
as published by the Free Software Foundation with the addition of the
following permission added to Section 15 as permitted in Section 7(a):
FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
OF THIRD PARTY RIGHTS

This program is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License
along with this program; if not, see http://www.gnu.org/licenses or write to
the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
Boston, MA, 02110-1301 USA, or download the license from the following URL:
http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions
of this program must display Appropriate Legal Notices, as required under
Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License,
a covered work must retain the producer line in every PDF that is created
or manipulated using iText.

You can be released from the requirements of the license by purchasing
a commercial license. Buying such a license is mandatory as soon as you
develop commercial activities involving the iText software without
disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP,
serving PDFs on the fly in a web application, shipping iText with a closed
source product.

For more information, please contact iText Software Corp. at this
address: sales@itextpdf.com
*/
using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509;
using iText.Signatures.Testutils;
using iText.Signatures.Testutils.Builder;
using iText.Test;
using iText.Test.Attributes;
using iText.Test.Signutils;

namespace iText.Signatures {
    public class OcspClientBouncyCastleTest : ExtendedITextTest {
        private static readonly String certsSrc = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/signatures/certs/";

        private static readonly char[] password = "testpass".ToCharArray();

        private static readonly String caCertFileName = certsSrc + "rootRsa.p12";

        private static X509Certificate checkCert;

        private static X509Certificate rootCert;

        private static TestOcspResponseBuilder builder;

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
        }

        [NUnit.Framework.SetUp]
        public virtual void SetUp() {
            X509Certificate caCert = (X509Certificate)Pkcs12FileHelper.ReadFirstChain(caCertFileName, password)[0];
            ICipherParameters caPrivateKey = Pkcs12FileHelper.ReadFirstKey(caCertFileName, password, password);
            builder = new TestOcspResponseBuilder(caCert, caPrivateKey);
            checkCert = (X509Certificate)Pkcs12FileHelper.ReadFirstChain(certsSrc + "signCertRsa01.p12", password)[0];
            rootCert = builder.GetIssuerCert();
        }

        [NUnit.Framework.Test]
        public virtual void GetBasicOCSPRespTest() {
            OcspClientBouncyCastle ocspClientBouncyCastle = CreateOcspClient();
            BasicOcspResp basicOCSPResp = ocspClientBouncyCastle.GetBasicOCSPResp(checkCert, rootCert, null);
            NUnit.Framework.Assert.IsNotNull(basicOCSPResp);
            NUnit.Framework.Assert.IsTrue(basicOCSPResp.Responses.Length > 0);
        }

        [NUnit.Framework.Test]
        public virtual void GetBasicOCSPRespNullTest() {
            OCSPVerifier ocspVerifier = new OCSPVerifier(null, null);
            OcspClientBouncyCastle ocspClientBouncyCastle = new OcspClientBouncyCastle(ocspVerifier);
            BasicOcspResp basicOCSPResp = ocspClientBouncyCastle.GetBasicOCSPResp(checkCert, null, null);
            NUnit.Framework.Assert.IsNull(basicOCSPResp);
        }

        [NUnit.Framework.Test]
        [LogMessage("OCSP response could not be verified")]
        public virtual void GetBasicOCSPRespLogMessageTest() {
            OcspClientBouncyCastle ocspClientBouncyCastle = CreateOcspClient();
            BasicOcspResp basicOCSPResp = ocspClientBouncyCastle.GetBasicOCSPResp(null, null, null);
            NUnit.Framework.Assert.IsNull(basicOCSPResp);
        }

        [NUnit.Framework.Test]
        public virtual void GetEncodedTest() {
            OcspClientBouncyCastle ocspClientBouncyCastle = CreateOcspClient();
            byte[] encoded = ocspClientBouncyCastle.GetEncoded(checkCert, rootCert, null);
            NUnit.Framework.Assert.IsNotNull(encoded);
            NUnit.Framework.Assert.IsTrue(encoded.Length > 0);
        }

        private static OcspClientBouncyCastle CreateOcspClient() {
            OCSPVerifier ocspVerifier = new OCSPVerifier(null, null);
            return new OcspClientBouncyCastleTest.TestOcspClientBouncyCastle(ocspVerifier);
        }

        private sealed class TestOcspClientBouncyCastle : OcspClientBouncyCastle {
            public TestOcspClientBouncyCastle(OCSPVerifier verifier)
                : base(verifier) {
            }

            internal override OcspResp GetOcspResponse(X509Certificate chCert, X509Certificate rCert, String url) {
                try {
                    CertificateID id = SignTestPortUtil.GenerateCertificateId(rootCert, checkCert.SerialNumber, Org.BouncyCastle.Ocsp.CertificateID.HashSha1
                        );
                    BasicOcspResp basicOCSPResp = builder.MakeOcspResponseObject(SignTestPortUtil.GenerateOcspRequestWithNonce
                        (id).GetEncoded());
                    return new OCSPRespGenerator().Generate(Org.BouncyCastle.Asn1.Ocsp.OcspResponseStatus.Successful, basicOCSPResp
                        );
                }
                catch (Exception e) {
                    throw new OcspException(e.Message);
                }
            }
        }
    }
}
