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

using System;
using Duplicati.Library.Interface;
using System.Collections.Generic;
using System.Linq;
using Duplicati.Library.Localization.Short;

namespace Duplicati.Library.Backend.OpenStack
{
    public class SwiftConfig : IWebModule
    {
        private const ConfigType DEFAULT_CONFIG_TYPE = ConfigType.Providers;
        private static readonly string DEFAULT_CONFIG_TYPE_STR = Enum.GetName(typeof(ConfigType), DEFAULT_CONFIG_TYPE);
        private const string KEY_CONFIGTYPE = "openstack-config";

        public enum ConfigType
        {
            Providers,
            Versions,
        }
        public SwiftConfig()
        {
        }

        public System.Collections.Generic.IDictionary<string, string> Execute(System.Collections.Generic.IDictionary<string, string> options)
        {
            string k;
            options.TryGetValue(KEY_CONFIGTYPE, out k);
            if (string.IsNullOrWhiteSpace(k))
                k = DEFAULT_CONFIG_TYPE_STR;

            ConfigType ct;
            if (!Enum.TryParse<ConfigType>(k, true, out ct))
                ct = DEFAULT_CONFIG_TYPE;

            switch (ct)
            {
                case ConfigType.Versions:
                    return OpenStack.OpenStackStorage.OPENSTACK_VERSIONS.ToDictionary((x) => x.Key, (y) => y.Value);
                default:
                    return OpenStack.OpenStackStorage.KNOWN_OPENSTACK_PROVIDERS.ToDictionary((x) => x.Key, (y) => y.Value);
            }
        }

        public string Key { get { return "openstack-getconfig"; } }

        public string DisplayName { get { return LC.L("OpenStack configuration module"); } }

        public string Description { get { return LC.L("Expose OpenStack configuration as a web module"); } }


        public System.Collections.Generic.IList<ICommandLineArgument> SupportedCommands
        {
            get
            {
                return new List<ICommandLineArgument>([
                    new CommandLineArgument(KEY_CONFIGTYPE, CommandLineArgument.ArgumentType.Enumeration, LC.L("The config to get"), LC.L("Provide different config values"), DEFAULT_CONFIG_TYPE_STR, Enum.GetNames(typeof(ConfigType)))

                ]);
            }
        }
    }
}
