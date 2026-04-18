namespace Definitions
{
    /// <summary>
    /// A reference is a wrapper around a definition that allows for lazy loading of the definition and json serialization as a name. It can be initialized with either the name of the definition or the definition itself. When the Get method is called, it will return the definition instance, loading it from the registered definitions if it has not already been loaded. If the name is null, Get will return null. This allows for references to definitions that may not exist at the time of reference creation, and for definitions to be loaded on demand.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Reference<T> where T : Definition
    {
        /// <summary>
        /// The name of the definition that this reference points to. Setting the name will invalidate the cached instance, causing it to be reloaded from the registered definitions the next time Get is called. If the name is null, Get will return null. This allows for references to definitions that may not exist at the time of reference creation, and for definitions to be loaded on demand.
        /// </summary>
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

        /// <summary>
        /// Gets the definition instance that this reference points to. If the instance has already been loaded and the name has not changed, it returns the cached instance. If the name is null, it returns null. Otherwise, it loads the definition from the registered definitions using the name and caches it before returning it. This allows for lazy loading of definitions and for references to be created before the definitions they point to are registered.
        /// </summary>
        /// <returns></returns>
        public T? Get()
        {
            if (instance != null && instance.Name == name)
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
