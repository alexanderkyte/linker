using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

using System.Runtime.CompilerServices;

namespace Mono.Linker.Steps {
	public class AccessAnnotatorStep : BaseStep
	{
		AssemblyDefinition assembly;

		MethodReference noOptAttr;
		MethodImplOptions noOptAttrArg;
		TypeReference noOptAttrArgType;

		protected override void ProcessAssembly (AssemblyDefinition assembly)
		{
			if (Annotations.GetAction (assembly) != AssemblyAction.Link)
				return;

			this.assembly = assembly;

			foreach (var type in assembly.MainModule.Types)
				ProcessType (type);
		}

		void ProcessType (TypeDefinition type)
		{
			foreach (var method in type.Methods) {
				if (method.HasBody)
					ProcessMethod (method);
			}

			foreach (var nested in type.NestedTypes)
				ProcessType (nested);
		}

		void ProcessMethod (MethodDefinition method)
		{
			// Public methods have non-visible call sites by default, this attribute doesn't
			// help us at all.
			//
			// See mono_aot_can_specialize in aot-compiler.c in mono
			if (!method.IsPrivate)
				return;

			if (!Annotations.IsReflected(method))
				return;

			//if (noOptAttr == null) {
			//	TypeDefinition methodImpl = BCL.FindPredefinedType ("System.Runtime.CompilerServices", "MethodImplAttribute", Context);
			//	TypeDefinition option = BCL.FindPredefinedType ("System.Runtime.CompilerServices", "MethodImplOptions", Context);
			//	TypeDefinition cecilInt16 = BCL.FindPredefinedType ("System", "Int16", Context);

			//	MethodDefinition reflectionMethod = null;

			//	foreach (var ref_method in methodImpl.Methods) {
			//		Console.WriteLine("Checking {0} as an option for constructor", ref_method.FullName);
			//		if (!ref_method.IsConstructor)
			//			continue;
			//		if (ref_method.Parameters.Count != 1)
			//			continue;
			//		if (ref_method.Parameters[0].ParameterType == cecilInt16) {
			//			reflectionMethod = ref_method;
			//			break;
			//		}
			//	}

			//	if (reflectionMethod == null)
			//		throw new Exception ("Could not find the Int16 constructor for MethodImplAttribute");

			//	noOptAttr = assembly.MainModule.ImportReference (reflectionMethod);
			//	noOptAttrArgType = assembly.MainModule.ImportReference (cecilInt16);

			//	if (noOptAttr == null)
			//		throw new Exception(String.Format("ImportReference failed on BCL type {0}", reflectionMethod));
			//	if (noOptAttrArgType == null)
			//		throw new Exception(String.Format("ImportReference failed on Int16"));
			//}


			// var reflectionAttr = new CustomAttribute (noOptAttr);
			// reflectionAttr.ConstructorArguments.Add (new CustomAttributeArgument (noOptAttrArgType, Mono.Cecil.MethodImplAttributes.NoOptimization));

			method.ImplAttributes |= MethodImplAttributes.NoOptimization;
			// method.CustomAttributes.Add (reflectionAttr);
		}
	}
}
