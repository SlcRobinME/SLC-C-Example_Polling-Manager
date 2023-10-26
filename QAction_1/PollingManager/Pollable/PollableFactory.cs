namespace Skyline.PollingManager.Pollable
{
    using System;
    using System.Reflection;

    using Skyline.DataMiner.Scripting;

    public class PollableFactory
    {
        public PollableBase CreatePollableBase(SLProtocol protocol, object[] row, Type type)
        {
            ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(SLProtocol), typeof(object[]) });

            if (constructor != null)
                return (PollableBase)constructor.Invoke(new object[] { protocol, row });
            else
                throw new Exception($"Type [{type.Name}] doesn't contain constructor that takes SLProtocol and object[]!");
        }
    }
}
