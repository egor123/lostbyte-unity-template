using System;

namespace Lostbyte.Toolkit.Injection
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class Initialize : Attribute { }
}
