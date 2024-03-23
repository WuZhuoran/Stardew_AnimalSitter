using System.Collections.Generic;

namespace AnimalSitter.Common
{
    internal interface IStats
    {
        IDictionary<string, object> GetFields();
    }
}
