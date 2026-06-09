using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Olib.Godot.Definitions.Tests;

public class TestSerialization
{
    private readonly ITestOutputHelper output;

    public TestSerialization(ITestOutputHelper output)
    {
        this.output = output;
        DefinitionTypeResolver.RegisterType(typeof(MyTestDefinition));
        DefinitionTypeResolver.RegisterType(typeof(TestDefinitionWithReference));
    }

    [Fact]
    public void SerializeDefinition()
    {
        var registry = new DefinitionRegistry();
        var options = new JsonSerializerOptions
        {
            Converters = { new ReferenceJsonConverter(registry), new DefinitionJsonConverterFactory(registry) },
            TypeInfoResolver = new DefinitionTypeResolver(),
            WriteIndented = true,
        };

        var originalDef = new MyTestDefinition("test_def_005")
        {
            MyProperty = "Hello, Serialization!"
        };
        registry.Register(originalDef);

        string json = JsonSerializer.Serialize(originalDef, options);
        Assert.False(string.IsNullOrEmpty(json));
        output.WriteLine(json);
        Assert.Equal("""
        {
          "$type": "MyTestDefinition",
          "MyProperty": "Hello, Serialization!",
          "Id": "test_def_005"
        }
        """, json);
    }

    [Fact]
    public void DeserializeDefinition()
    {
        var registry = new DefinitionRegistry();
        var options = new JsonSerializerOptions
        {
            Converters = { new ReferenceJsonConverter(registry), new DefinitionJsonConverterFactory(registry) },
            TypeInfoResolver = new DefinitionTypeResolver(),
        };

        string json = """
        {
          "$type": "MyTestDefinition",
          "MyProperty": "Hello, Deserialization!",
          "Id": "test_def_006"
        }
        """;

        var deserializedDef = JsonSerializer.Deserialize<MyTestDefinition>(json, options);
        Assert.NotNull(deserializedDef);
        Assert.IsType<MyTestDefinition>(deserializedDef);
        Assert.Equal("test_def_006", deserializedDef.Id);
        Assert.Equal("Hello, Deserialization!", deserializedDef.MyProperty);
        Assert.Equal(deserializedDef, registry.Get<MyTestDefinition>("test_def_006"));
    }

    [Fact]
    public void SerializeDefinitionWithReference()
    {
        var registry = new DefinitionRegistry();
        var options = new JsonSerializerOptions
        {
            Converters = { new ReferenceJsonConverter(registry), new DefinitionJsonConverterFactory(registry) },
            TypeInfoResolver = new DefinitionTypeResolver(),
            WriteIndented = true,
        };
        var referencedDef = new MyTestDefinition("referenced_def_001")
        {
            MyProperty = "I am the referenced definition."
        };
        registry.Register(referencedDef);

        var originalDef = new TestDefinitionWithReference("test_def_007")
        {
            MyReference = new Reference<MyTestDefinition>("referenced_def_001", registry)
        };
        registry.Register(originalDef);

        string json = JsonSerializer.Serialize<TestDefinitionWithReference>(originalDef, options);
        Assert.False(string.IsNullOrEmpty(json));
        output.WriteLine(json);

        Assert.Equal("""
        {
          "$type": "TestDefinitionWithReference",
          "MyReference": "referenced_def_001",
          "Id": "test_def_007"
        }
        """, json);
    }

    [Fact]
    public void DeserializeDefinitionWithReference()
    {
        var registry = new DefinitionRegistry();
        var options = new JsonSerializerOptions
        {
            Converters = { new ReferenceJsonConverter(registry), new DefinitionJsonConverterFactory(registry) },
            TypeInfoResolver = new DefinitionTypeResolver(),
        };

        string json = """
        {
          "$type": "TestDefinitionWithReference",
          "MyReference": "referenced_def_001",
          "Id": "test_def_007"
        }
        """;

        string refJson = """
        {
          "$type": "MyTestDefinition",
          "MyProperty": "I am the referenced definition.",
          "Id": "referenced_def_001"
        }
        """;

        var deserializedRefDef = JsonSerializer.Deserialize<MyTestDefinition>(refJson, options);
        Assert.NotNull(deserializedRefDef);
        Assert.IsType<MyTestDefinition>(deserializedRefDef);

        var deserializedDef = JsonSerializer.Deserialize<TestDefinitionWithReference>(json, options);
        Assert.NotNull(deserializedDef);
        Assert.IsType<TestDefinitionWithReference>(deserializedDef);

        Assert.Equal("test_def_007", deserializedDef.Id);
        Assert.Equal("referenced_def_001", deserializedDef.MyReference.Id);
        Assert.Equal(deserializedDef, registry.Get<TestDefinitionWithReference>("test_def_007"));
        Assert.Equal(deserializedRefDef, deserializedDef.MyReference.Get());
    }

    public class MyTestDefinition : Definition
    {
        public string MyProperty { get; set; } = string.Empty;

        public MyTestDefinition(string id) : base(id)
        {
        }
    }

    public class TestDefinitionWithReference : Definition
    {
        public Reference<MyTestDefinition> MyReference { get; set; }

        public TestDefinitionWithReference(string id) : base(id)
        {
            MyReference = new Reference<MyTestDefinition>(null, null!);
        }
    }
}
