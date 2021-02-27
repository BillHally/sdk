// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

internal class StartupHook
{
    private static readonly ApplyUpdateDelegate? _applyHotReloadUpdate = GetApplyUpdate();

    public static void Initialize()
    {
        // Ignore this
        _ = Task.Run(async () =>
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_AUTO_RELOAD_WS_ENDPOINT")!;

            var client = new ClientWebSocket();
            await client.ConnectAsync(new Uri(env), default);
            var buffer = new byte[4 * 1024 * 1024];
            while (client.State == WebSocketState.Open)
            {
                Console.WriteLine("Waiting for message.");
                var receive = await client.ReceiveAsync(buffer, default);
                if (receive.CloseStatus is not null)
                {
                    Console.WriteLine(receive.CloseStatus);
                    break;
                }

                Console.WriteLine("Message received.");


                UpdatePayload update;
                try
                {
                    update = JsonSerializer.Deserialize<UpdatePayload>(buffer.AsSpan(0, receive.Count), new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                }
                catch (Exception ex)
                {
                    // Ignore these. It's probably a message for the browser.
                    continue;
                }

                if (update.Type != "UpdateCompilation")
                {
                    continue;
                }
                Console.WriteLine("Attempting to apply diff.");
                try
                {
                    foreach (var item in update.Deltas)
                    {
                        _applyHotReloadUpdate(typeof(Program).Assembly, item.MetadataDelta, item.ILDelta, ReadOnlySpan<byte>.Empty);
                    }

                    Console.WriteLine("Applied diff");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            Console.WriteLine("Exited update loop");
        });
    }

    private static void ApplyChangesToAssembly(string assemblyName, byte[] deltaMeta, byte[] deltaIl)
    {
        if (_applyHotReloadUpdate is null)
        {
            throw new NotSupportedException("ApplyUpdate is not supported.");
        }

        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((a) => !a.IsDynamic && Path.GetFileName(a.Location) == assemblyName);
        _applyHotReloadUpdate(assembly, new ReadOnlySpan<byte>(deltaMeta), new ReadOnlySpan<byte>(deltaIl), ReadOnlySpan<byte>.Empty);
    }

    private static ApplyUpdateDelegate? GetApplyUpdate()
    {
        var applyUpdateMethod = typeof(System.Reflection.Metadata.AssemblyExtensions).GetMethod("ApplyUpdate", BindingFlags.Public | BindingFlags.Static);
        if (applyUpdateMethod is null)
        {
            return null;
        }

        var applyHotReloadUpdate = (ApplyUpdateDelegate)applyUpdateMethod.CreateDelegate(typeof(ApplyUpdateDelegate))!;
        return applyHotReloadUpdate;
    }

    private delegate void ApplyUpdateDelegate(Assembly assembly, ReadOnlySpan<byte> metadataDelta, ReadOnlySpan<byte> ilDelta, ReadOnlySpan<byte> pdbDelta);

    private struct UpdatePayload
    {
        public string Type { get; set; }

        public IEnumerable<UpdateDelta> Deltas { get; set; }
    }

    private struct UpdateDelta
    {
        public string ModulePath { get; set; }
        public byte[] MetadataDelta { get; set; }
        public byte[] ILDelta { get; set; }
    }
}
