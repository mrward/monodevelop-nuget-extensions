// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.VisualStudio
{
	/// <summary>
	/// This class unifies all the different ways of getting services.
	/// </summary>
	public static class ServiceLocator
	{
		public static void InitializePackageServiceProvider (IServiceProvider provider)
		{
			if (provider == null) {
				throw new ArgumentNullException (nameof (provider));
			}

			PackageServiceProvider = provider;
		}

		public static IServiceProvider PackageServiceProvider { get; private set; }

		public static TService GetInstanceSafe<TService> () where TService : class
		{
			try {
				return GetInstance<TService> ();
			} catch (Exception) {
				return null;
			}
		}

		public static TService GetInstance<TService> () where TService : class
		{
			return GetGlobalService<TService, TService> ();
		}

		public static TInterface GetGlobalService<TService, TInterface> () where TInterface : class
		{
			if (PackageServiceProvider != null) {
				var result = PackageServiceProvider.GetService (typeof (TService));
				TInterface service = result as TInterface;
				if (service != null) {
					return service;
				}
			}

			return null;
		}
	}
}
