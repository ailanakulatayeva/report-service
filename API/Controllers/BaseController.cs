using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.RegularExpressions;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/hse/reporting-service/[controller]")]
public class BaseController<T>(ILogger<T> logger) : ControllerBase
{
}
/// <summary>
/// Расширения для формирования IActionResult из FluentResult
/// </summary>
public static class ResultExtension
{
    /// <summary>
    /// Формирования IActionResult из FluentResult
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static IActionResult Respond(this Result result)
        => result.IsSuccess ? new OkResult() : HandleErrorResult(result);

    /// <summary>
    /// Формирования IActionResult из FluentResult&lt;T&gt;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    public static IActionResult Respond<T>(this Result<T> result)
        => result.IsSuccess ? new OkObjectResult(result.Value) : HandleErrorResult(result);

    private static IActionResult HandleErrorResult(this IResultBase result)
    {
        // Значения по умолчанию
        int finalStatusCode = StatusCodes.Status500InternalServerError;
        int currentPriority = int.MaxValue;

        foreach (var error in result.Errors)
        {
            // 1. Определяем приоритет и статус код для текущей ошибки
            (int priority, int status) = error switch
            {
                //ExternalError => (1, 518),
                //InternalServerError => (2, StatusCodes.Status500InternalServerError),
                //DeserializationError => (30, StatusCodes.Status500InternalServerError),
                //ConflictError => (40, StatusCodes.Status409Conflict),
                //BarrierError => (41, StatusCodes.Status428PreconditionRequired),
                //ForbiddenError => (50, StatusCodes.Status403Forbidden),
                //NotFoundError => (60, StatusCodes.Status404NotFound),
                //BadRequestError => (70, StatusCodes.Status400BadRequest),
                //ValidationError => (71, StatusCodes.Status400BadRequest),
                //Warning => (72, StatusCodes.Status207MultiStatus),
                _ => (90, StatusCodes.Status500InternalServerError)
            };

            // 2. Если нашли ошибку важнее, чем та, что была найдена ранее — запоминаем её
            if (priority < currentPriority)
            {
                currentPriority = priority;
                finalStatusCode = status;
            }

            // Микро-оптимизация: если нашли самый высокий приоритет (1), 
            // дальше искать нет смысла, выходим из цикла.
            if (currentPriority == 1) break;
        }

        return new ObjectResult(result.Errors) { StatusCode = finalStatusCode };
    }
}

public class HyphenateRouteTokenTransformer : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            // Transform controller names from PascalCase to kebab-case
            controller.ControllerName = ConvertToKebabCase(controller.ControllerName);
        }
    }

    private string ConvertToKebabCase(string str)
    {
        return Regex.Replace(str, "([a-z0-9])([A-Z])", "$1-$2").ToLower();
    }
}
