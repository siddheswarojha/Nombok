using System;
using System.Reflection;
using System.Reflection.Emit;

public static class GetterSetterProcessor
{
    public static void Process(Type type)
    {
        try
        {
            var assemblyName = new AssemblyName("DynamicAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
            var typeBuilder = moduleBuilder.DefineType(type.FullName + "Extensions", TypeAttributes.Public);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(property, typeof(GetterSetterAttribute)))
                {
                    GenerateGetterSetterMethods(typeBuilder, property);
                }
            }

            var generatedType = typeBuilder.CreateType();
            Console.WriteLine($"Dynamic type '{generatedType.Name}' with getter and setter properties created.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating getter and setter properties: {ex.Message}");
        }
    }

    private static void GenerateGetterSetterMethods(TypeBuilder typeBuilder, PropertyInfo property)
    {
        var fieldBuilder = typeBuilder.DefineField("_" + property.Name, property.PropertyType, FieldAttributes.Private);

        var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);

        var getMethodBuilder = typeBuilder.DefineMethod("get_" + property.Name, MethodAttributes.Public | MethodAttributes.HideBySig, property.PropertyType, Type.EmptyTypes);
        var getIL = getMethodBuilder.GetILGenerator();
        getIL.Emit(OpCodes.Ldarg_0);
        getIL.Emit(OpCodes.Ldfld, fieldBuilder);
        getIL.Emit(OpCodes.Ret);

        var setMethodBuilder = typeBuilder.DefineMethod("set_" + property.Name, MethodAttributes.Public | MethodAttributes.HideBySig, null, new Type[] { property.PropertyType });
        var setIL = setMethodBuilder.GetILGenerator();
        setIL.Emit(OpCodes.Ldarg_0);
        setIL.Emit(OpCodes.Ldarg_1);
        setIL.Emit(OpCodes.Stfld, fieldBuilder);
        setIL.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getMethodBuilder);
        propertyBuilder.SetSetMethod(setMethodBuilder);

        Console.WriteLine($"Property '{property.Name}' with getter and setter methods created.");
    }
}
