using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DopplerCustomDomain.CustomDomainProvider;

namespace DopplerCustomDomain.Api
{
    public class DomainConfiguration
    {
        public string service { get; set; } = string.Empty;
        [EnumDataType(typeof(RuleType))]
        public RuleType ruleType { get; set; }
    }
}
