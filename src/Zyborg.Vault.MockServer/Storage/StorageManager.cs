using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Zyborg.Vault.MockServer.Storage
{
    public class StorageManager
    {
        public static readonly IReadOnlyDictionary<string, Type> StorageTypes =
                new Dictionary<string, Type>
                {
                    ["in-memory"] = typeof(InMemoryStorage),
                    // ["file"] = typeof(FileStorage),
                    // ["json-file"] = typeof(JsonFileStorage),
                };
                
        private ILogger _logger;
        private StorageSettings _settings = new StorageSettings();

        private IStorage _storage;

        public StorageManager(ILogger<StorageManager> logger, IConfiguration config)
        {
            _logger = logger;

            config.Bind(typeof(StorageManager).FullName, _settings);
        }

        public void Init(IApplicationBuilder app)
        {
            if (!StorageTypes.TryGetValue(_settings.Type, out var storageType))
                throw new NotSupportedException($"unsupported storage type: {_settings.Type}");

            IStorage s = (IStorage)ActivatorUtilities.CreateInstance(app.ApplicationServices, storageType);
            if (s == null)
                throw new NotSupportedException($"unresolved storage type: {_settings.Type}: {storageType}");
            _storage = s;

            var stateJson = await _storage.ReadAsync("server/state");
            if (stateJson != null)
            {
                State.Durable = JsonConvert.DeserializeObject<DurableServerState>(
                        stateJson);
                Health.Initialized = true;
            }
        }

        public class StorageSettings
        {
            public string Type { get; set; } = "file";
        }
    }
}