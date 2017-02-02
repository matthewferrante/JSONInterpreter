using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTMS.Core.Api {
    public class Manifest {
        #region Fields
        private dynamic _config;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Manifest"/> class.
        /// </summary>
        /// <param name="config">The data.</param>
        public Manifest(dynamic config) {
            Load(config);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the version.
        /// </summary>
        /// <value>The version.</value>
        public int Version { get; private set; }

        /// <summary>
        /// Returns the check interval.
        /// </summary>
        /// <value>The check interval.</value>
        public int CheckInterval { get; private set; }

        /// <summary>
        /// Returns the configuration URI.
        /// </summary>
        /// <value>The remote configuration URI.</value>
        public string ConfigUri { get; private set; }

        /// <summary>
        /// Returns the security token.
        /// </summary>
        /// <value>The security token.</value>
        public string SecurityToken { get; private set; }

        /// <summary>
        /// Returns the base URI.
        /// </summary>
        /// <value>The base URI.</value>
        public string BaseUri { get; private set; }

        /// <summary>
        /// Manifest Payloads
        /// </summary>
        /// <value>The payload.</value>
        public string[] Payloads { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Parses the Config
        /// </summary>
        /// <param name="config">The Configuration Data to parse.</param>
        private void Load(dynamic config) {
            _config = config;

            // Set properties.
            Version = config.Version;
            CheckInterval = config.CheckInterval;
            SecurityToken = config.SecurityToken;
            ConfigUri = config.ConfigUri;
            BaseUri = config.BaseUri;
            Payloads = config.Payloads;
        }

        /// <summary>
        /// Writes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void Write(string path) {
            File.WriteAllText(path, _config);
        }
        #endregion
    }
}
