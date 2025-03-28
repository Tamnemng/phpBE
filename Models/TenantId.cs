using System;

namespace OMS.Core.Tenant
{
    public interface IMultiTenant
    {
        public Guid TenantId { get; set; }
    }
}
