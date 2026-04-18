# C# Definitions Library
This library provides a system for defining and registering definitions in C#. It allows for easy serialization and deserialization of definitions to and from JSON, as well as support for polymorphic types and reference handling. The library also includes events for when definitions are registered, when new definition types are encountered, and when definitions are replaced, allowing for easy integration with other systems that may need to react to changes in the definitions.

This library could be useful for games, where you can load game data from JSON files and want to have a flexible system for defining different types of game data (e.g. items, characters, quests, etc.) without needing to write custom code for each type. It could also be useful for any application that needs to manage a large number of definitions or configurations that can be easily serialized and deserialized.

## Usage
```csharp
using Definitions;

public class MyDefinition : Definition
{
    public string SomeProperty { get; set; }
}

public class MyOtherDefinition : Definition
{
    public int SomeOtherProperty { get; set; }
    public Reference<MyDefinition> ReferenceToMyDefinition { get; set; }
}

// Register a new definition
var myDefinition = new MyDefinition { Name = "MyDefinition1", SomeProperty = "Hello" };
var myOtherDefinition = new MyOtherDefinition { Name = "MyOtherDefinition1", SomeOtherProperty = 42, ReferenceToMyDefinition = new Reference<MyDefinition>(myDefinition) };

// Definitions are automatically registered

// Access definitions by type and name
myDefinition == Definition.Get<MyDefinition>("MyDefinition1"); // true
myOtherDefinition == Definition.Get<MyOtherDefinition>("MyOtherDefinition1"); // true

// Access referenced definitions
myDefinition == myOtherDefinition.ReferenceToMyDefinition.Get(); // true

// Serialize definitions to JSON
string json = myDefinition.ToJson(); // {"$type":"mydefinition","Name":"MyDefinition1","SomeProperty":"Hello"}
string otherJson = myOtherDefinition.ToJson(); // {"$type":"myotherdefinition","Name":"MyOtherDefinition1","SomeOtherProperty":42,"ReferenceToMyDefinition":""myDefinition"}

// parse definitions from JSON
Definition.Parse(json, "debugpath", out var parsedDefinition); // returns a list of parsed definitions, in this case just one definition of type MyDefinition with the same properties as the original definition
Definition.Parse(otherJson, "debugpath", out var parsedOtherDefinition); // returns a list of parsed definitions, in this case just one definition of type MyOtherDefinition with the same properties as the original definition, and the reference to MyDefinition is resolved to the original MyDefinition instance

// Since the newly parsed definitions have the same name and type as the original definitions, they replace the existing ones. This can be prevented by setting SkipDuplicateDefinitions to true, which will cause the new definitions to be ignored instead of replacing the existing ones.
Definition.SkipDuplicateDefinitions = true;

// You can also load a list of definitions from a JSON file. The file should contain an array of definitions, each with a "$type" property that indicates the type of the definition. The library will automatically deserialize the definitions and register them.
// [ {"$type":"mydefinition","Name":"MyDefinition1","SomeProperty":"Hello"},{"$type":"myotherdefinition","Name":"MyOtherDefinition1","SomeOtherProperty":42,"ReferenceToMyDefinition":""myDefinition"} ]
Definition.Parse(jsonList, "debugpath", out var parseDefinitionList);

```

## Advanced usage
```csharp
// You can also subscribe to the events for when definitions are registered, when new definition types are encountered, and when definitions are replaced. This allows you to react to changes in the definitions, such as updating a UI or triggering other actions in your application.
Definition.DefinitionRegistered += (sender, args) =>
{
    Console.WriteLine($"Definition registered: {args.Definition.Name} of type {args.Definition.GetType().Name}");
};
Definition.NewDefinitionTypeEncountered += (sender, args) =>
{
    Console.WriteLine($"New definition type encountered: {args.Definition.GetType().Name}");
};
Definition.DefinitionReplaced += (sender, args) =>
{
    Console.WriteLine($"Definition replaced: {args.Definition.Name} of type {args.Definition.GetType().Name}");
};

// You can customize a class before creation or deserialization by overriding the BeforeRegistration() method. This allows you to perform any necessary setup or validation before the definition is registered or serialized.
public class MyGenericDefinition : Definition
{
    public string SomeProperty { get; set; }

    // this method is called upon creation or deserialization, before the definition is registered. You can use this method to perform any necessary setup or validation before the definition is registered or serialized.
    public override void BeforeRegistration()
    {
        Name = Guid.NewGuid().ToString(); // Assign a unique name to the definition before registration. I've found this is useful for save game data, where you want to ensure that each definition has a unique name, but you don't want to manually assign names to each definition in the JSON file. By generating a unique name before registration, you can ensure that each definition is registered without conflicts, even if the JSON file contains multiple definitions of the same type without names.
    }
}
```

## AI Warning
Generative AI has only been used to help write the README.md file and documentation, while the code itself is written and tested entirely by a human.
