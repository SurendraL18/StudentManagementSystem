using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StudentManagement.API.Swagger
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // 1. Inspect both the local method and the enclosing class controller metadata for [Authorize] rules
            var hasAuthorizeAttribute = context.MethodInfo.DeclaringType != null && (
                context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any());

            // 2. Safely fall back to check if global MVC authorization filters are present in the pipeline state
            //var hasGlobalFilter = context.ApiDescription.ActionDescriptor.FilterDescriptors
            //    .Any(filter => filter.Filter is AuthorizeFilter);

            // 3. Early exit if the endpoint permits public access via [AllowAnonymous] overriding attributes
            var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ||
                                    (context.MethodInfo.DeclaringType != null &&
                                     context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any());

            if (hasAuthorizeAttribute && !hasAllowAnonymous)
            {
                // 4. Initialize the operational security response requirement list if it's empty
                operation.Security ??= new List<OpenApiSecurityRequirement>();

                // 5. Structure the explicit link reference back to the root "Bearer" scheme key definition
                var securityScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                // 6. Append the security payload mapping requirement rule to this single operation block
                operation.Security.Add(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });
            }
        }
    }
}
