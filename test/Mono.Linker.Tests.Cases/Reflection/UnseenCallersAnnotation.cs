using System;
using System.Reflection;
using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Metadata;

namespace Mono.Linker.Tests.Cases.Reflection
{
	[SetupLinkerArgument("--annotate-unseen-callers")]
	public class UnseenCallersAnnotation
	{
		static Type attrType;


		public static void Main()
		{
			var typeA = typeof(A);
			var method = typeA.GetMethod("Foo", BindingFlags.Public);
			method.Invoke(null, new object[] { });
		}

		[Kept]
		public class A
		{
			[Kept]
			[KeptAttributeAttribute("System.Runtime.CompilerServices.ReflectionBlockedAttribute")]
			public int Foo()
			{
				return 42;
			}
		}

	}
}
