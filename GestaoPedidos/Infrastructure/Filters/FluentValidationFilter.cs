using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GestaoPedidos.Infrastructure.Filters
{
    public sealed class FluentValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var failures = new List<ValidationFailure>();

            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null)
                {
                    continue;
                }

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                if (context.HttpContext.RequestServices.GetService(validatorType)
                    is not IValidator validator)
                {
                    continue;
                }

                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(
                    validationContext,
                    context.HttpContext.RequestAborted);

                failures.AddRange(result.Errors);
            }

            if (failures.Count == 0)
            {
                await next();
                return;
            }

            var errors = failures
                .GroupBy(failure => string.IsNullOrWhiteSpace(failure.PropertyName)
                    ? "$"
                    : failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(failure => failure.ErrorMessage)
                        .Distinct()
                        .ToArray());

            var problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Um ou mais erros de validação ocorreram.",
                Type = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1",
                Instance = context.HttpContext.Request.Path
            };

            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            context.Result = new BadRequestObjectResult(problem)
            {
                ContentTypes = { "application/problem+json" }
            };
        }
    }
}
