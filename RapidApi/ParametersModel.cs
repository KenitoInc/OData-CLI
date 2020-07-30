using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ODataCLI
{
    class ParametersModel
    {
        [JsonPropertyName("$schema")]
        public string Schema { get; set; }

        [JsonPropertyName("contentVersion")]
        public string ContentVersion { get; set; }

        [JsonPropertyName("parameters")]
        public InnerParameters Parameters { get; set; }

    }

    class InnerParameters
    {
        [JsonPropertyName("sqlAdministratorLogin")]
        public ParameterValue SqlAdminstratorLogin { get; set; }

        [JsonPropertyName("sqlAdministratorLoginPassword")]
        public ParameterValue SqlAdminstratorLoginPassword { get; set; }

        [JsonPropertyName("siteName")]
        public ParameterValue SiteName { get; set; }

        [JsonPropertyName("sqlServerName")]
        public ParameterValue SqlServerName { get; set; }

        [JsonPropertyName("databaseName")]
        public ParameterValue DatabaseName { get; set; }
    }

    class ParameterValue
    {
        public ParameterValue(string value)
        {
            Value = value;
        }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
