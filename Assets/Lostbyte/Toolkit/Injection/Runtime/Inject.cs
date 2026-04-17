using System;

namespace Lostbyte.Toolkit.Injection
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public class Inject : Attribute { }
}
