using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DopplerCustomDomain.CustomDomainProvider
{
    public class DomainConfiguration
    {
        public string service { get; set; } = string.Empty;
        [EnumDataType(typeof(RuleType))]
        public RuleType ruleType { get; set; }
    }
}
