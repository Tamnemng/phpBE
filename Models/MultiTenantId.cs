using System;
using System.Text.Json.Serialization;
using OMS.Core.Tenant;

namespace OMS.Core.Commands
{
    public abstract class MultiTenantCommand : Command, IMultiTenant
    {
        [JsonIgnore]
        public Guid TenantId { get; set; }
    }

    public abstract class MultiTenantCommand<T> : Command<T>, IMultiTenant
    {
        [JsonIgnore]
        public Guid TenantId { get; set; }
    }
}
