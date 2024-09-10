using Heming;
using Microsoft.AspNetCore.Mvc;

namespace HemingDetect.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PredictionController : Controller
    {
        IPredicting predicting;
        public PredictionController(IPredicting predicting)
        {
            this.predicting = predicting;
        }

        [HttpPost]
        public IActionResult Index()
        {
            IFormFileCollection formFiles = Request.Form.Files;
            if (formFiles.Any())
            {
                Stream fs = formFiles[0].OpenReadStream();
                var predictions = predicting.LocalPredicting(fs);
                Console.WriteLine($"Send predicting result {Json(predictions).ToString()}");
                return Json(predictions);
            }
            else
                return BadRequest();
        }
    }
}
