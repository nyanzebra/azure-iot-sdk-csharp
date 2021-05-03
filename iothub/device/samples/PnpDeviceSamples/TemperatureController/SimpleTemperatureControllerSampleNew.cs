﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Client.Samples
{
    public class SimpleTemperatureControllerSampleNew
    {
        private static readonly Random s_random = new();

        private readonly DeviceClient _deviceClient;
        private readonly ILogger _logger;

        public SimpleTemperatureControllerSampleNew(DeviceClient deviceClient, ILogger logger)
        {
            _deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient), $"{nameof(deviceClient)} cannot be null.");

            if (logger == null)
            {
                using ILoggerFactory loggerFactory = LoggerFactory.Create(builer => builer.AddConsole());
                _logger = loggerFactory.CreateLogger<SimpleTemperatureControllerSampleNew>();
            }
            else
            {
                _logger = logger;
            }
        }

        public async Task PerformOperationsAsync(CancellationToken cancellationToken)
        {
            // Retrieve the device's properties.
            ClientProperties properties = await _deviceClient.GetClientPropertiesAsync(cancellationToken);

            // Verify if the device has previously reported a value for property "serialNumber".
            // If the expected value has not been previously reported then report it.
            string serialNumber = "SR-12345";
            if (!properties.Contains("serialNumber")
                || properties.Get<string>("serialNumber") != serialNumber)
            {
                var propertiesToBeUpdated = new ClientPropertyCollection
                {
                    ["serialNumber"] = serialNumber
                };
                await _deviceClient.UpdateClientPropertiesAsync(propertiesToBeUpdated, cancellationToken);
                _logger.LogDebug($"Property: Update - {propertiesToBeUpdated.GetSerializedString()} in KB.");
            }

            // Send telemetry "workingSet".
            long workingSet = Process.GetCurrentProcess().PrivateMemorySize64 / 1024;
            using var message = new TelemetryMessage
            {
                MessageId = s_random.Next().ToString(),
                Telemetry = { ["workingSet"] = workingSet },
            };
            await _deviceClient.SendTelemetryAsync(message, cancellationToken);
            _logger.LogDebug($"Telemetry: Sent - {message.Telemetry.GetSerializedString()} in KB.");

            // Subscribe and respond to event for writable property "targetHumidity".
            await _deviceClient.SubscribeToWritablePropertiesEventAsync(
                async (writableProperties, userContext) =>
                {
                    string propertyName = "targetHumidity";
                    if (!writableProperties.Contains(propertyName))
                    {
                        _logger.LogDebug($"Property: Update - Received a property update which is not implemented.\n{writableProperties.GetSerializedString()}");
                        return;
                    }

                    double targetHumidity = writableProperties.GetValue<double>(propertyName);

                    var propertyPatch = new ClientPropertyCollection();
                    propertyPatch.Add(
                        propertyName,
                        targetHumidity,
                        (int)StatusCode.Completed,
                        writableProperties.Version,
                        "The operation completed successfully.");

                    await _deviceClient.UpdateClientPropertiesAsync(propertyPatch, cancellationToken);
                    _logger.LogDebug($"Property: Update - \"{propertyPatch.GetSerializedString()}\" is complete.");
                },
                null,
                cancellationToken);

            // Subscribe and respond to command "reboot".
            await _deviceClient.SubscribeToCommandsAsync(
                async (commandRequest, userContext) =>
                {
                    try
                    {
                        switch (commandRequest.CommandName)
                        {
                            case "reboot":
                                int delay = commandRequest.GetData<int>();
                                _logger.LogDebug($"Command: Received - Rebooting thermostat (resetting temperature reading to 0°C after {delay} seconds).");

                                await Task.Delay(TimeSpan.FromSeconds(delay));
                                _logger.LogDebug($"Command: Rebooting thermostat (resetting temperature reading to 0°C after {delay} seconds) has {StatusCode.Completed}.");

                                return new CommandResponse((int)StatusCode.Completed);

                            default:
                                _logger.LogWarning($"Received a command request that isn't implemented - command name = {commandRequest.CommandName}");
                                return new CommandResponse((int)StatusCode.NotFound);
                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        _logger.LogDebug($"Command input is invalid: {ex.Message}.");
                        return new CommandResponse((int)StatusCode.BadRequest);
                    }
                },
                null,
                cancellationToken);

            Console.ReadKey();
        }
    }
}