using Xunit;

namespace Olib.Godot.Definitions.Tests;

public class TestDefinitions
{
    [Fact]
    public void CreateDefinition()
    {
        var registry = new DefinitionRegistry();
        var def = new MyTestDefinition("test_def_001")
        {
            MyProperty = "Hello, World!"
        };
        Assert.Equal("test_def_001", def.Id);
    }

    [Fact]
    public void CreateDefinitionWithReferences()
    {
        var registry = new DefinitionRegistry();
        var def = new MyTestDefinitionWithReferences("test_def_002")
        {
            MyReference = new Reference<MyReferencedTestDefinition>("ref_def_001", registry)
        };
        registry.Register(def);

        var referencedDef = new MyReferencedTestDefinition("ref_def_001")
        {
            MyProperty = "I am a referenced definition."
        };
        registry.Register(referencedDef);

        Assert.Equal("test_def_002", def.Id);
        Assert.NotNull(def.MyReference);
        Assert.Equal(referencedDef, def.MyReference!.Get());
    }

    [Fact]
    public void TestDuplicateDefinitionError()
    {
        var registry = new DefinitionRegistry();
        //registry.SkipDuplicateDefinitions = false;
        //registry.ReplaceDuplicateDefinitions = false;

        var def1 = new MyTestDefinition("test_def_003");
        var def2 = new MyTestDefinition("test_def_003");

        registry.Register(def1);
        Assert.Throws<InvalidOperationException>(() => registry.Register(def2));
    }

    [Fact]
    public void TestDuplicateDefinitionSkip()
    {
        var registry = new DefinitionRegistry();
        registry.SkipDuplicateDefinitions = true;

        var def1 = new MyTestDefinition("test_def_004");
        var def2 = new MyTestDefinition("test_def_004");

        registry.Register(def1);
        registry.Register(def2);

        var retrievedDef = registry.Get<MyTestDefinition>("test_def_004");
        Assert.Equal(def1, retrievedDef);
    }

    [Fact]
    public void TestDuplicateDefinitionReplace()
    {
        var registry = new DefinitionRegistry();
        registry.ReplaceDuplicateDefinitions = true;

        var def1 = new MyTestDefinition("test_def_005") { MyProperty = "First" };
        var def2 = new MyTestDefinition("test_def_005") { MyProperty = "Second" };

        registry.Register(def1);
        registry.Register(def2);

        var retrievedDef = registry.Get<MyTestDefinition>("test_def_005");
        Assert.Equal(def2, retrievedDef);
        Assert.Equal("Second", retrievedDef!.MyProperty);
    }

    [Fact]
    public void TestSameNameDifferentType()
    {
        var registry = new DefinitionRegistry();

        var def1 = new MyTestDefinition("test_def_006") { MyProperty = "I am a MyTestDefinition." };
        var def2 = new MyReferencedTestDefinition("test_def_006") { MyProperty = "I am a MyReferencedTestDefinition." };

        registry.Register(def1);
        registry.Register(def2);

        var retrievedDef1 = registry.Get<MyTestDefinition>("test_def_006");
        var retrievedDef2 = registry.Get<MyReferencedTestDefinition>("test_def_006");

        Assert.Equal(def1, retrievedDef1);
        Assert.Equal(def2, retrievedDef2);
    }

    class MyTestDefinition : Definition
    {
        public string MyProperty { get; set; } = string.Empty;

        public MyTestDefinition(string id) : base(id)
        {
        }
    }

    class MyTestDefinitionWithReferences : Definition
    {
        public Reference<MyReferencedTestDefinition>? MyReference { get; set; }

        public MyTestDefinitionWithReferences(string id) : base(id)
        {
        }
    }

    class MyReferencedTestDefinition : Definition
    {
        public string MyProperty { get; set; } = string.Empty;

        public MyReferencedTestDefinition(string id) : base(id)
        {
        }
    }
}
