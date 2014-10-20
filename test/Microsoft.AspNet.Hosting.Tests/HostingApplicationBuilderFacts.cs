// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;
using Microsoft.AspNet.PipelineCore;
using Microsoft.AspNet.RequestContainer;
using Microsoft.AspNet.Hosting.Builder;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using System.Linq;
using System.Reflection;
using System;

namespace Microsoft.AspNet.Hosting.Tests
{
    public class HostingApplicationBuilderFacts
    {
        public static bool FoundRequestServices { get; set; } = false;

        [RequiresServices]
        public class RequireServicesMiddleware {
            private readonly RequestDelegate _next;

            public RequireServicesMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            [RequiresServices]
            public static Task Invoke([RequiresServices] HttpContext context)
            {
                //return _next.Invoke(context);
            }
        }

        [Fact]
        public void VerifyRequiresServicesAttributeCanBeFound()
        {
            Assert.NotNull(typeof(RequireServicesMiddleware).GetTypeInfo().GetCustomAttribute<RequiresServicesAttribute>(true));
            Assert.NotNull(typeof(RequireServicesMiddleware).GetMethod("Invoke").GetCustomAttribute<RequiresServicesAttribute>(true));

            Func<RequestDelegate, RequestDelegate> foo = (next) => RequireServicesMiddleware.Invoke;
            Assert.NotNull(foo.Method.);
        }


        [Fact]
        public void RequestServicesAvailableWhenTargetHasRequiresServices()
        {
            var baseServiceProvider = new ServiceCollection()
                .Add(HostingServices.GetDefaultServices())
                .BuildServiceProvider();
            var builder = new HostingApplicationBuilder(baseServiceProvider);

            bool foundRequestServicesBefore = false;
            builder.Use(next => async c =>
            {
                foundRequestServicesBefore = c.RequestServices != null;
                await next.Invoke(c);
            });

            builder.UseMiddleware<RequireServicesMiddleware>();

            bool foundRequestServicesAfter = false;
            builder.Use(next => async c =>
            {
                foundRequestServicesAfter = c.RequestServices != null;
                await next.Invoke(c);
            });

            var context = new DefaultHttpContext();
            builder.Build().Invoke(context);
            Assert.False(foundRequestServicesBefore);
            Assert.True(foundRequestServicesAfter);
        }
    }
}