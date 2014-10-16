// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Builder;
using System.Reflection;

namespace Microsoft.AspNet.Hosting.Builder
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RequiresServicesAttribute : Attribute { }

    public class HostingApplicationBuilder : ApplicationBuilder
    {
        public HostingApplicationBuilder(IServiceProvider services) : base(services) { }

        private bool _addedServices = false;

        public override IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            if (!_addedServices && middleware.Target.GetType().GetTypeInfo().GetCustomAttribute<RequiresServicesAttribute>(true) != null)
            {
                this.UseRequestServices();
                _addedServices = true;
            }

            return base.Use(middleware);
        }
    }

    public class ApplicationBuilderFactory : IApplicationBuilderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ApplicationBuilderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IApplicationBuilder CreateBuilder()
        {
            return new HostingApplicationBuilder(_serviceProvider);
        }
    }
}
