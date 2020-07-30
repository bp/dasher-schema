using Dasher.Schema.Generation.TestClasses;

namespace Dasher.Schema.Generation.TestRefAssembly
{
    //dummy class to reference any of the classes from TestClasses in order to have assembly reference.
    public class Dummy
    {
#pragma warning disable 169
        private DummySerialisable _serializable;
#pragma warning restore 169
    }
}
