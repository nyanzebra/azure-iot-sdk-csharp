﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Client
{
    /// <summary>
    ///
    /// </summary>
    public class WritablePropertyResponse
    {
        private readonly PropertyConvention _propertyConvention;

        /// <summary>
        /// Convenience constructor for specifying only the property value.
        /// </summary>
        /// <param name="propertyValue">The unserialized property value.</param>
        /// <param name="propertyConvention"></param>
        public WritablePropertyResponse(object propertyValue, PropertyConvention propertyConvention)
        {
            // null checks

            Value = propertyValue;
            _propertyConvention = propertyConvention;
        }

        /// <summary>
        /// Convenience constructor for specifying the properties.
        /// </summary>
        /// <param name="propertyValue">The unserialized property value.</param>
        /// <param name="ackCode">The acknowledgement code, usually an HTTP Status Code e.g. 200, 400.</param>
        /// <param name="ackVersion">The acknowledgement version, as supplied in the property update request.</param>
        /// <param name="ackDescription">The acknowledgement description, an optional, human-readable message about the result of the property update.</param>
        /// <param name="propertyConvention"></param>
        public WritablePropertyResponse(object propertyValue, int ackCode, long ackVersion, string ackDescription = default, PropertyConvention propertyConvention = default)
        {
            // null checks

            Value = propertyValue;
            AckCode = ackCode;
            AckVersion = ackVersion;
            AckDescription = ackDescription;

            _propertyConvention = propertyConvention;
        }

        /// <summary>
        /// The unserialized property value.
        /// </summary>
        [JsonIgnore]
        public object Value { get; private set; }

        /// <summary>
        /// The acknowledgement code, usually an HTTP Status Code e.g. 200, 400.
        /// </summary>
        [JsonProperty("ac")]
        public int AckCode { get; set; }

        /// <summary>
        /// The acknowledgement version, as supplied in the property update request.
        /// </summary>
        [JsonProperty("av")]
        public long AckVersion { get; set; }

        /// <summary>
        /// The acknowledgement description, an optional, human-readable message about the result of the property update.
        /// </summary>
        [JsonProperty("ad", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AckDescription { get; set; }

        /// <summary>
        /// The serialized property value.
        /// </summary>
        [JsonProperty("value")]
        public JRaw ValueAsJson => new JRaw(_propertyConvention.SerializeToString(Value));
    }
}