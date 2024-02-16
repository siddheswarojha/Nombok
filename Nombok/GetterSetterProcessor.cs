using System;
using System.Reflection;
using System.Reflection.Emit;

public static class GetterSetterProcessor
{
    public static void Process(object obj)
    {
        Type type = obj.GetType();

        AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
        TypeBuilder typeBuilder = moduleBuilder.DefineType(type.FullName + "Extensions", TypeAttributes.Public, type);

        foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (Attribute.IsDefined(field, typeof(GetterSetterAttribute)))
            {
                GenerateProperty(typeBuilder, field);
            }
        }

        Type generatedType = typeBuilder.CreateType();
        Activator.CreateInstance(generatedType);
    }

    private static void GenerateProperty(TypeBuilder typeBuilder, FieldInfo field)
    {
        PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(field.Name, PropertyAttributes.None, field.FieldType, null);

        MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        // Generate the getter method
        MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_" + field.Name, getSetAttr, field.FieldType, Type.EmptyTypes);
        ILGenerator getIL = getMethodBuilder.GetILGenerator();
        getIL.Emit(OpCodes.Ldarg_0);
        getIL.Emit(OpCodes.Ldfld, field);
        getIL.Emit(OpCodes.Ret);

        // Generate the setter method
        MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_" + field.Name, getSetAttr, null, new Type[] { field.FieldType });
        ILGenerator setIL = setMethodBuilder.GetILGenerator();
        setIL.Emit(OpCodes.Ldarg_0);
        setIL.Emit(OpCodes.Ldarg_1);
        setIL.Emit(OpCodes.Stfld, field);
        setIL.Emit(OpCodes.Ret);

        // Assign the getter and setter methods to the property
        propertyBuilder.SetGetMethod(getMethodBuilder);
        propertyBuilder.SetSetMethod(setMethodBuilder);

        Console.WriteLine($"Property '{field.Name}' with getter and setter methods created.");
    }
}
