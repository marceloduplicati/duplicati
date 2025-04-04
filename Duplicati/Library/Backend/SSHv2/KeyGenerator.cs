// Copyright (C) 2025, The Duplicati Team
// https://duplicati.com, hello@duplicati.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

using System.Security.Cryptography;
using Duplicati.Library.Interface;
using System.Text;

namespace Duplicati.Library.Backend
{
    public class KeyGenerator : IWebModule
    {
        private const string KEY_TYPE_NAME = "key-type";
        private const string KEY_USERNAME = "key-username";
        private const string KEY_KEYLEN = "key-bits";

        private const string KEY_TEMPLATE_DSA = "-----BEGIN DSA PRIVATE KEY-----\n{0}\n-----END DSA PRIVATE KEY----- \n";
        private const string KEY_TEMPLATE_RSA = "-----BEGIN RSA PRIVATE KEY-----\n{0}\n-----END RSA PRIVATE KEY----- \n";
        private const string PUB_KEY_FORMAT_DSA = "ssh-dss";
        private const string PUB_KEY_FORMAT_RSA = "ssh-rsa";

        private const string KEYTYPE_DSA = "dsa";
        private const string KEYTYPE_RSA = "rsa";

        private readonly string DEFAULT_USERNAME = "backup-user@" + System.Environment.MachineName;
        private const string DEFAULT_KEYTYPE = KEYTYPE_DSA;
        private const int DEFAULT_KEYLEN = 1024;

        private static void EncodePEMLength(System.IO.Stream s, uint length)
        {
            s.WriteByte((byte)((length >> 24) & 0xff));
            s.WriteByte((byte)((length >> 16) & 0xff));
            s.WriteByte((byte)((length >> 8) & 0xff));
            s.WriteByte((byte)(length & 0xff));
        }

        private static void EncodeDERLength(System.IO.Stream s, uint length)
        {
            if (length < 0x7f)
            {
                s.WriteByte((byte)length);
            }
            else if (length <= 0x7fff)
            {
                s.WriteByte(0x82);
                s.WriteByte((byte)((length >> 8) & 0xff));
                s.WriteByte((byte)(length & 0xff));
            }
            else
            {
                s.WriteByte(0x84);
                s.WriteByte((byte)((length >> 24) & 0xff));
                s.WriteByte((byte)((length >> 16) & 0xff));
                s.WriteByte((byte)((length >> 8) & 0xff));
                s.WriteByte((byte)(length & 0xff));
            }
        }

        private static byte[] EncodeDER(byte[][] data)
        {
            byte[] payload;
            using (var ms = new MemoryStream())
            {
                foreach (var b in data)
                {
                    ms.WriteByte(0x02);
                    var isNegative = (b[0] & 0x80) != 0;

                    EncodeDERLength(ms, (uint)(b.Length + (isNegative ? 1 : 0)));
                    if (isNegative)
                        ms.WriteByte(0);
                    ms.Write(b, 0, b.Length);
                }

                payload = ms.ToArray();
            }

            using (var ms = new MemoryStream())
            {
                ms.WriteByte(0x30);
                EncodeDERLength(ms, (uint)payload.Length);
                ms.Write(payload, 0, payload.Length);
                return ms.ToArray();
            }
        }

        private static byte[] EncodePEM(byte[][] data)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                foreach (var n in data)
                {
                    var isNegative = (n[0] & 0x80) != 0;
                    EncodePEMLength(ms, (uint)(n.Length + (isNegative ? 1 : 0)));
                    if (isNegative)
                        ms.WriteByte(0);
                    ms.Write(n, 0, n.Length);
                }
                return ms.ToArray();
            }
        }

        private static IDictionary<string, string> OutputKey(byte[] private_key, byte[] public_key, string pem_template, string key_name, string username)
        {
            var res = new Dictionary<string, string>();

            var b64_raw = Convert.ToBase64String(private_key);
            var sb = new StringBuilder();
            var lw = 64;
            for (var i = 0; i < b64_raw.Length; i += lw)
            {
                sb.Append(b64_raw.Substring(i, Math.Min(lw, b64_raw.Length - i)));
                sb.Append("\n");
            }

            var b64 = sb.ToString().Trim();
            var pem = string.Format(pem_template, b64);
            var uri = SSHv2.KEYFILE_URI + Duplicati.Library.Utility.Uri.UrlEncode(pem);
            var pub = key_name + " " + Convert.ToBase64String(public_key) + " " + username;

            res["privkey"] = b64_raw;
            res["privkeyfile"] = pem;
            res["privkeyuri"] = uri;
            res["pubkey"] = pub;
            return res;
        }

        #region IWebModule implementation
        public IDictionary<string, string> Execute(IDictionary<string, string> options)
        {
            if (!options.TryGetValue(KEY_TYPE_NAME, out var keytype))
                keytype = DEFAULT_KEYTYPE;

            if (!options.TryGetValue(KEY_USERNAME, out var username))
                username = DEFAULT_USERNAME;

            if (!options.TryGetValue(KEY_KEYLEN, out var keylen_s))
                keylen_s = "0";

            if (!int.TryParse(keylen_s, out var keylen))
                keylen = DEFAULT_KEYLEN;

            if (KEYTYPE_RSA.Equals(keytype, StringComparison.OrdinalIgnoreCase))
            {
                var rsa = RSACryptoServiceProvider.Create();
                if (keylen > 0)
                    rsa.KeySize = keylen;
                else
                    rsa.KeySize = DEFAULT_KEYLEN;

                var key = rsa.ExportParameters(true);

                var privateEntries = new byte[][] { new byte[] { 0x0 }, key.Modulus ?? [], key.Exponent ?? [], key.D ?? [], key.P ?? [], key.Q ?? [], key.DP ?? [], key.DQ ?? [], key.InverseQ ?? [] };
                var publicEntries = new byte[][] {
                    Encoding.ASCII.GetBytes(PUB_KEY_FORMAT_RSA),
                    key.Exponent ?? [],
                    key.Modulus ?? []
                };

                return OutputKey(EncodeDER(privateEntries), EncodePEM(publicEntries), KEY_TEMPLATE_RSA, PUB_KEY_FORMAT_RSA, username);
            }
            else if (KEYTYPE_DSA.Equals(keytype, StringComparison.OrdinalIgnoreCase))
            {
                var dsa = DSACryptoServiceProvider.Create();
                if (keylen > 0)
                    dsa.KeySize = keylen;
                else
                    dsa.KeySize = DEFAULT_KEYLEN;

                var key = dsa.ExportParameters(true);

                var privateEntries = new byte[][] { new byte[] { 0x0 }, key.P ?? [], key.Q ?? [], key.G ?? [], key.Y ?? [], key.X ?? [] };
                var publicEntries = new byte[][] {
                    Encoding.ASCII.GetBytes(PUB_KEY_FORMAT_DSA),
                    key.P ?? [],
                    key.Q ?? [],
                    key.G ?? [],
                    key.Y ?? []
                };

                return OutputKey(EncodeDER(privateEntries), EncodePEM(publicEntries), KEY_TEMPLATE_DSA, PUB_KEY_FORMAT_DSA, username);
            }
            else
            {
                throw new UserInformationException(string.Format("Unsupported key type: {0}", keytype), "SSHUnsupportedKey");
            }
        }
        public string Key => "ssh-keygen";

        public string DisplayName => Strings.KeyGenerator.DisplayName;
        public string Description => Strings.KeyGenerator.Description;
        public IList<ICommandLineArgument> SupportedCommands => [
            new CommandLineArgument(KEY_KEYLEN, CommandLineArgument.ArgumentType.Integer, Strings.KeyGenerator.KeyLenShort, Strings.KeyGenerator.KeyLenLong, DEFAULT_KEYLEN.ToString(), null, new string[] {"1024", "2048"}),
            new CommandLineArgument(KEY_TYPE_NAME, CommandLineArgument.ArgumentType.Enumeration, Strings.KeyGenerator.KeyTypeShort, Strings.KeyGenerator.KeyTypeLong, DEFAULT_KEYTYPE, null, new string[] {KEYTYPE_DSA, KEYTYPE_RSA}),
            new CommandLineArgument(KEY_USERNAME, CommandLineArgument.ArgumentType.Integer, Strings.KeyGenerator.KeyUsernameShort, Strings.KeyGenerator.KeyUsernameLong, DEFAULT_USERNAME),
        ];
        #endregion
    }
}

