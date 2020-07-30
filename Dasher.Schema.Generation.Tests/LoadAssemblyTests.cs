using Dasher.Schema.Generation.TestRefAssembly;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Dasher.Schema.Generation.Tests
{
    public class AssemblyWalkerProxy
    {
        private readonly AssemblyWalker _assemblyWalker;
        private readonly System.Type _type;

        public AssemblyWalkerProxy(AssemblyWalker assemblyWalker)
        {
            _assemblyWalker = assemblyWalker;
            _type = _assemblyWalker.GetType();
        }

        public List<string> ExcludedPrefixes => (List<string>)_type.GetField("ExcludedPrefixes", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_assemblyWalker);
        public List<string> ExcludedAssemblies => (List<string>)_type.GetField("ExcludedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_assemblyWalker);
        public List<string> IncludedPrefixes => (List<string>)_type.GetField("IncludedPrefixes", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_assemblyWalker);
        public List<string> IncludedAssemblies => (List<string>)_type.GetField("IncludedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_assemblyWalker);

        public List<AssemblyName> GetFilteredReferencedAssemblyNames(AssemblyName[] referencedAssemblies)
        {
            return (List<AssemblyName>) _type.GetMethod("GetFilteredReferencedAssemblyNames", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_assemblyWalker, new object[] { referencedAssemblies });
        }
    }

    public class LoadAssemblyTests
    {
        private readonly DasherAssemblyInfo _dasherAssemblyInfo;
        private readonly AssemblyWalkerProxy _assemblyWalkerProxy;

        public LoadAssemblyTests()
        {
            var assembly = typeof(Dummy).Assembly;
            var assemblyWalker = new AssemblyWalker("Dasher.Schema.Generation.*,Something", "Microsoft.*,Another");
            _assemblyWalkerProxy = new AssemblyWalkerProxy(assemblyWalker);
            _dasherAssemblyInfo = assemblyWalker.GetDasherAssemblyInfo(assembly);
        }

        [Fact]
        public void LoadSerialisables()
        {
            Assert.Equal(3, _dasherAssemblyInfo.DeserialisableTypes.Count);
            Assert.Equal(4, _dasherAssemblyInfo.SerialisableTypes.Count);
        }


        [Fact]
        public void SerialiseTest()
        {
            Assert.Contains(_dasherAssemblyInfo.SerialisableTypes, o => o.Name == "DummySerialisable");
            Assert.Contains(_dasherAssemblyInfo.SerialisableTypes, o => o.Name == "DummySerialisable");
            Assert.Contains(_dasherAssemblyInfo.SerialisableTypes, o => o.Name == "BaseSerialiseDeserialise");
            Assert.Contains(_dasherAssemblyInfo.SerialisableTypes, o => o.Name == "DummySerialiseDeserialise");
            Assert.Contains(_dasherAssemblyInfo.SerialisableTypes, o => o.Name == "DerivedSerialiseOnly");
            Assert.DoesNotContain(_dasherAssemblyInfo.SerialisableTypes, o => o.Name == "DummyDeserialiseOnly");
        }

        [Fact]
        public void DeserialiseTest()
        {
            Assert.Contains(_dasherAssemblyInfo.DeserialisableTypes, o => o.Name == "DummyDeserialiseOnly");
            Assert.Contains(_dasherAssemblyInfo.DeserialisableTypes, o => o.Name == "BaseSerialiseDeserialise");
            Assert.Contains(_dasherAssemblyInfo.DeserialisableTypes, o => o.Name == "DummySerialiseDeserialise");
            Assert.DoesNotContain(_dasherAssemblyInfo.DeserialisableTypes, o => o.Name == "DerivedSerialiseOnly");
            Assert.DoesNotContain(_dasherAssemblyInfo.DeserialisableTypes, o => o.Name == "DummySerialisable");
        }
        [Fact]
        public void BaseClassIgnoreTest()
        {
            Assert.DoesNotContain(_dasherAssemblyInfo.DeserialisableTypes, o => o.Name == "DerivedSerialiseOnly");
            Assert.Contains(_dasherAssemblyInfo.SerialisableTypes, o => o.Name == "DerivedSerialiseOnly");
        }

        [Fact]
        public void IncludedListsTest()
        {
            var ep = _assemblyWalkerProxy.ExcludedPrefixes;
            var ea = _assemblyWalkerProxy.ExcludedAssemblies;
            var ip = _assemblyWalkerProxy.IncludedPrefixes;
            var ia = _assemblyWalkerProxy.IncludedAssemblies;
            Assert.NotNull(ep);
            Assert.NotNull(ea);
            Assert.NotNull(ip);
            Assert.NotNull(ia);
            Assert.Equal(3, ep.Count);
            Assert.Equal(3, ea.Count);
            Assert.Single(ip);
            Assert.Single(ia);
        }

        [Fact]
        public void FilterReferencedAssembliesTest()
        {
            var aw = new AssemblyWalker("IncludedAssembly,Included.*", "ExcludedAssembly,Excluded.*");
            var proxy = new AssemblyWalkerProxy(aw);
            var result = proxy.GetFilteredReferencedAssemblyNames(GetTestAssemblies());
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Included.Assembly", result[0].Name);
            Assert.Equal("IncludedAssembly", result[1].Name);
        }

        [Fact]
        public void FilterReferencedAssembliesExcludedTest()
        {
            var aw = new AssemblyWalker(null, "ExcludedAssembly,Excluded.*");
            var proxy = new AssemblyWalkerProxy(aw);
            var result = proxy.GetFilteredReferencedAssemblyNames(GetTestAssemblies());
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Included.Assembly", result[0].Name);
            Assert.Equal("IncludedAssembly", result[1].Name);
        }

        [Fact]
        public void FilterReferencedAssembliesIncludedTest()
        {
            var aw = new AssemblyWalker("IncludedAssembly,Included.*", null);
            var proxy = new AssemblyWalkerProxy(aw);
            var result = proxy.GetFilteredReferencedAssemblyNames(GetTestAssemblies());
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Included.Assembly", result[0].Name);
            Assert.Equal("IncludedAssembly", result[1].Name);
        }        

        [Fact]
        public void FilterReferencedAssembliesEmptyTest()
        {
            var aw = new AssemblyWalker(null, null);
            var proxy = new AssemblyWalkerProxy(aw);
            var result = proxy.GetFilteredReferencedAssemblyNames(GetTestAssemblies());
            Assert.NotNull(result);
            Assert.Equal(4, result.Count());
            Assert.Equal("Included.Assembly", result[0].Name);
            Assert.Equal("IncludedAssembly", result[1].Name);
        }

        [Fact]
        public void FilterReferencedAssembliesOneExcludedTest()
        {
            var aw = new AssemblyWalker(null, "ExcludedAssembly");
            var proxy = new AssemblyWalkerProxy(aw);
            var result = proxy.GetFilteredReferencedAssemblyNames(GetTestAssemblies());
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Equal("Included.Assembly", result[0].Name);
            Assert.Equal("IncludedAssembly", result[1].Name);
            Assert.Equal("Excluded.Assembly", result[2].Name);
        }

        [Fact]
        public void FilterReferencedAssembliesOneIncludedTest()
        {
            var aw = new AssemblyWalker("IncludedAssembly", "ExcludedAssembly,Excluded.*");
            var proxy = new AssemblyWalkerProxy(aw);
            var result = proxy.GetFilteredReferencedAssemblyNames(GetTestAssemblies());
            Assert.NotNull(result);
            Assert.Single(result);            
            Assert.Equal("IncludedAssembly", result[0].Name);
        }

        [Fact]
        public void FilterReferencedAssembliesIncludedNotExistTest()
        {
            var aw = new AssemblyWalker("NotExist", "ExcludedAssembly,Excluded.*");
            var proxy = new AssemblyWalkerProxy(aw);
            var result = proxy.GetFilteredReferencedAssemblyNames(GetTestAssemblies());
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void FilterReferencedAssembliesIncludedNotExistPrefixTest()
        {
            var aw = new AssemblyWalker("NotExist*", "ExcludedAssembly,Excluded.*");
            var proxy = new AssemblyWalkerProxy(aw);
            var result = proxy.GetFilteredReferencedAssemblyNames(GetTestAssemblies());
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void FilterReferencedAssembliesIncludedNotExistBothTest()
        {
            var aw = new AssemblyWalker("NotExist*,NotExist", "ExcludedAssembly,Excluded.*");
            var proxy = new AssemblyWalkerProxy(aw);
            var result = proxy.GetFilteredReferencedAssemblyNames(GetTestAssemblies());
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        private static AssemblyName[] GetTestAssemblies()
        {
            AssemblyName[] assemblies =
            {
                new AssemblyName("Dasher"), new AssemblyName("System"), new AssemblyName("System.Core"),
                new AssemblyName("Microsoft"),
                new AssemblyName("Microsoft.Bill"), new AssemblyName("Included.Assembly"), new AssemblyName("IncludedAssembly"),
                new AssemblyName("ExcludedAssembly"), new AssemblyName("Excluded.Assembly"),
            };
            return assemblies;
        }
    }
}