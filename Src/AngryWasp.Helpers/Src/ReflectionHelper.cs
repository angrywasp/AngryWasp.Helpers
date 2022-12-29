using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AngryWasp.Helpers
{
    [Flags]
    public enum Property_Access_Mode
    {
        Read = 1,
        Write = 2
    }

    [Flags]
    public enum Field_Access_Mode
    {
        Private = 1,
        Public = 2
    }

    public enum Object_Type
    {
        None,
        List,
        Dictionary,
        Array,
        Class,
        Struct,
        Primitive,
        Enum
    }

    //todo: the default behaviour of GetFields and GetProperties is to get the top most and ignore base implementations
    //for example, if we have a field in a base class and a derived class with the same name only the field in the derived class is
    //included. This should be changed to include both versions. This however will require keys in the dictionary to be the fully qualified name
    //of the member, which in turn will require changes to how these methods are implemented and their results consumed

    public class ReflectionHelper
    {
        private Dictionary<string, Assembly> assemblyCache;
        private Dictionary<string, Type> assemblyTypeCache;

        public Dictionary<string, Type> AssemblyTypeCache
        {
            get { return assemblyTypeCache; }
        }

        public Dictionary<string, MethodInfo> GetMethods(Type type)
        {
            MethodInfo[] mi = type.GetMethods();

            Dictionary<string, MethodInfo> returnValue = new Dictionary<string, MethodInfo>();

            foreach (var m in mi)
                returnValue.Add(m.Name, m);

            return returnValue;
        }

        #region GetFields

        public Dictionary<string, FieldInfo> GetFields(Type type, Field_Access_Mode access)
        {
            Dictionary<string, FieldInfo> fd = new Dictionary<string, FieldInfo>();

            GetFieldsInternal(type, fd, access);

            Type baseType = type.BaseType;

            while (baseType != null && baseType != typeof(object))
            {
                GetFieldsInternal(baseType, fd, access);
                baseType = baseType.BaseType;
            }

            return fd;
        }

        private void GetFieldsInternal(Type type, Dictionary<string, FieldInfo> fd, Field_Access_Mode access)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                Field_Access_Mode fam = GetAccess(field);

                if (!access.HasFlag(fam))
                    continue;

                if (fd.ContainsKey(field.Name))
                    continue;

                fd.Add(field.Name, field);
            }
        }

        private Field_Access_Mode GetAccess(FieldInfo field)
        {
            if (field.IsPublic)
                return Field_Access_Mode.Public;

            return Field_Access_Mode.Private;
        }

        #endregion

        #region GetProperties

        public Dictionary<string, PropertyInfo> GetProperties(Type type, Property_Access_Mode access)
        {
            Dictionary<string, PropertyInfo> pd = new Dictionary<string, PropertyInfo>();

            GetPropertiesInternal(type, pd, access);

            Type baseType = type.BaseType;

            while (baseType != null && baseType != typeof(object))
            {
                GetPropertiesInternal(baseType, pd, access);
                baseType = baseType.BaseType;
            }

            return pd;
        }

        private void GetPropertiesInternal(Type type, Dictionary<string, PropertyInfo> pd, Property_Access_Mode access)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (PropertyInfo property in properties)
            {
                Property_Access_Mode pam = GetAccess(property);

                if (!access.HasFlag(pam))
                    continue;

                if (pd.ContainsKey(property.Name))
                    continue;

                pd.Add(property.Name, property);
            }
        }

        private Property_Access_Mode GetAccess(PropertyInfo property)
        {
            Property_Access_Mode p = 0;

            if (property.CanRead && property.GetMethod.IsPublic)
                p |= Property_Access_Mode.Read;

            if (property.CanWrite && property.SetMethod.IsPublic)
                p |= Property_Access_Mode.Write;

            return p;
        }

        #endregion

        private static ReflectionHelper instance;

        public static ReflectionHelper Instance
        {
            get
            {
                if (instance == null)
                    instance = new ReflectionHelper();

                return instance;
            }
        }

        public static ReflectionHelper CreateInstance()
        {
            instance = new ReflectionHelper();
            return instance;
        }

        private ReflectionHelper()
        {
            assemblyCache = new Dictionary<string, Assembly>();
            assemblyTypeCache = new Dictionary<string, Type>();
        }

        public Object_Type GetObjectType(Type type)
        {
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                return Object_Type.Primitive;
            else if (type.IsEnum)
                return Object_Type.Enum;
            else if (type.IsClass)
            {
                if (type.IsArray)
                    return Object_Type.Array;
                else if (InheritsOrImplements(type, typeof(IList)))
                    return Object_Type.List;
                else if (InheritsOrImplements(type, typeof(IDictionary)))
                    return Object_Type.Dictionary;
                else
                    return Object_Type.Class;
            }
            else if (type.IsValueType)
                return Object_Type.Struct;

            return Object_Type.None;
        }

        /// <summary>
        /// gets a list of all the types in assembly that inherit or implement testType
        /// testType can be an interface of class type
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="testType"></param>
        /// <returns></returns>
        public List<Type> GetTypesInheritingOrImplementing(Assembly assembly, Type testType, bool includeBase = false)
        {
            List<Type> types = new List<Type>();

            if (includeBase)
                types.Add(testType);

            try
            {
                Type[] atypes = assembly.GetTypes();
                foreach (Type type in atypes)
                {
                    if (InheritsOrImplements(type, testType) && type != testType)
                        types.Add(type);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Exception[] loaderExceptions = ex.LoaderExceptions;
            }

            return types;
        }

        public bool InheritsOrImplements(Type child, Type parent)
        {
            var shouldUseGenericType = true;
            if (parent.IsGenericType && parent.GetGenericTypeDefinition() != parent)
                shouldUseGenericType = false;

            if (parent.IsGenericType && shouldUseGenericType)
                parent = parent.GetGenericTypeDefinition();

            var currentChild = child.IsGenericType
                                   ? child.GetGenericTypeDefinition()
                                   : child;

            while (currentChild != typeof(object))
            {
                bool hasAnyInterfaces = currentChild.GetInterfaces()
                .Any(childInterface =>
                    {
                        var currentInterface = childInterface.IsGenericType
                        ? childInterface.GetGenericTypeDefinition()
                        : childInterface;

                        return currentInterface == parent;
                    });

                if (parent == currentChild || hasAnyInterfaces)
                    return true;

                currentChild = currentChild.BaseType != null
                && currentChild.BaseType.IsGenericType
                                   ? currentChild.BaseType.GetGenericTypeDefinition()
                                   : currentChild.BaseType;

                if (currentChild == null)
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Checks the AssemblyCache for the file and returns it if it exists, otherwise it loads it to the cache and returns it
        /// </summary>
        /// <param name="p2"></param>
        /// <returns></returns>
        public Assembly LoadAssemblyFile(string f)
        {
            if (assemblyCache.ContainsKey(f))
                return assemblyCache[f];
            else
            {
                Assembly a = Assembly.LoadFrom(f);

                assemblyCache.Add(f, a);

                foreach (Type t in a.GetTypes())
                    assemblyTypeCache.Add(t.AssemblyQualifiedName, t);

                return a;
            }
        }

        /// <summary>
        /// Gets the default value of a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object GetDefault(Type type)
        {
            // If no Type was supplied, if the Type was a reference type, or if the Type was a System.Void, return null
            if (type == null || !type.IsValueType || type == typeof(void))
                return null;

            // If the supplied Type has generic parameters, its default value cannot be determined
            if (type.ContainsGenericParameters)
                throw new ArgumentException(
                    "{" + MethodInfo.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
                    "> contains generic parameters, so the default value cannot be retrieved");

            // If the Type is a primitive type, or if it is another publicly-visible value type (i.e. struct), return a
            //  default instance of the value type
            if (type.IsPrimitive || !type.IsNotPublic)
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        "{" + MethodInfo.GetCurrentMethod() + "} Error:\n\nThe Activator.CreateInstance method could not " +
                        "create a default instance of the supplied value type <" + type +
                        "> (Inner Exception message: \"" + e.Message + "\")", e);
                }
            }

            // Fail with exception
            throw new ArgumentException("{" + MethodInfo.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
                "> is not a publicly-visible type, so the default value cannot be retrieved");
        }
    }
}