using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;

namespace OMS.Core.Utilities
{
    public static class IdGenerator
    {
        /// <summary>
        /// Generates a unique ID of specified length
        /// </summary>
        public static string GenerateId(int length)
        {
            string timestamp = DateTime.UtcNow.Ticks.ToString();
            string guid = Guid.NewGuid().ToString("N");
            string combined = guid + timestamp;
            
            if (combined.Length > length)
            {
                return combined.Substring(0, length);
            }
            else
            {
                return combined.PadRight(length, 'X');
            }
        }
    }
}