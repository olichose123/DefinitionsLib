namespace Definitions
{
    /// <summary>
    /// A reference is a wrapper around a definition that allows for lazy loading of the definition and json serialization as a name. It can be initialized with either the name of the definition or the definition itself. When the Get method is called, it will return the definition instance, loading it from the registered definitions if it has not already been loaded. If the name is null, Get will return null. This allows for references to definitions that may not exist at the time of reference creation, and for definitions to be loaded on demand.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Reference<T> where T : Definition
    {
        public string? Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                instance = null;
            }
        }
        string? name;
        T? instance;

        public Reference(string? name)
        {
            this.name = name;
        }

        public Reference(T definition)
        {
            name = definition.Name;
            instance = definition;
        }

        public T? Get()
        {
            if (instance != null)
            {
                return instance;
            }
            if (name == null)
            {
                return null;
            }
            instance = Definition.Get<T>(name);
            return instance;
        }
    }
}
