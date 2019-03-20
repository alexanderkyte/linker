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
			if (!Annotations.IsReflected (method))
				return;

			// Public methods have non-visible call sites by default, this attribute doesn't
			// help us at all.
			//
			// See mono_aot_can_specialize in aot-compiler.c in mono
			if (!method.IsPrivate)
				return;

			if (noOptAttr == null) {
				var methodImpl = typeof (System.Runtime.CompilerServices.MethodImplAttribute);
				var option = typeof(System.Runtime.CompilerServices.MethodImplOptions);

				noOptAttrArg = (MethodImplOptions) Enum.Parse(option, "NoOptimization");
				var reflectionMethod = methodImpl.GetConstructor (new Type[] { typeof (Int16) });

				noOptAttr = assembly.MainModule.ImportReference (reflectionMethod);
				noOptAttrArgType = assembly.MainModule.ImportReference (typeof(Int16));
			}

			var reflectionAttr = new CustomAttribute (noOptAttr);
			reflectionAttr.ConstructorArguments.Add (new CustomAttributeArgument (noOptAttrArgType, (Int16) noOptAttrArg));

			method.CustomAttributes.Add (reflectionAttr);
		}
	}
}
