﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client.Samples
{
    internal class CustomPayloadConvention : PayloadConvention
    {
        public override Serializer PayloadSerializer => new CustomJsonSerializer();

        public override ContentEncoder PayloadEncoder => Utf8ContentEncoder.Instance;

        public override byte[] GetObjectBytes(object objectToSendWithConvention)
        {
            string serializedString = PayloadSerializer.SerializeToString(objectToSendWithConvention);
            return PayloadEncoder.EncodeStringToByteArray(serializedString);
        }
    }
}