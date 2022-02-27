using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InMemoryDB.Controllers
{
    public class ErrorController : Controller
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("/error-development")]
        public IActionResult HandleErrorDevelopment()
        {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            return Problem(
                detail: exceptionHandlerFeature.Error.Message
                //title: exceptionHandlerFeature.Error.Message,
                //detail: exceptionHandlerFeature.Error.StackTrace
            );
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("/error")]
        public IActionResult HandleError()
        {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            return Problem(
                detail: exceptionHandlerFeature.Error.Message
            );
        }
    }
}
