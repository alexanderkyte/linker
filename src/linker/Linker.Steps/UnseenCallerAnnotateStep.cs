﻿using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

using System.Runtime.CompilerServices;

namespace Mono.Linker.Steps {
	public class UnseenCallerAnnotateStep : BaseStep
	{
		AssemblyDefinition assembly;
		MethodReference noOptAttr;

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

			if (!Annotations.HasUnseenCallers(method))
				return;

			if (noOptAttr == null) {
				TypeDefinition methodImpl = BCL.FindPredefinedType("System.Runtime.CompilerServices", "ReflectionBlockedAttribute", Context);
				MethodDefinition reflectionMethod = null;
				foreach (var ref_method in methodImpl.Methods)
				{
					if (!ref_method.IsConstructor)
						continue;
					if (ref_method.Parameters.Count != 0)
						continue;
					reflectionMethod = ref_method;
				}
				noOptAttr = assembly.MainModule.ImportReference (reflectionMethod);
				if (noOptAttr == null)
					throw new Exception("Could not find System.Runtime.CompilerServices.ReflectionBlockedAttribute in BCL.");
			}
			var cattr = new CustomAttribute(noOptAttr);
			method.CustomAttributes.Add (cattr);

			Annotations.Mark(cattr);
			Annotations.Mark(cattr.AttributeType);
			Annotations.Mark(cattr.Constructor);
		}
	}
}
