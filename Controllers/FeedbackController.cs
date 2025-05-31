using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RemediEmr.Data.Class;
using RemediEmr.Repositry;
using static JwtService;

namespace RemediEmr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {

        private readonly FeedbackRepositry comrep;
        private readonly JwtHandler jwtHandler;

        public FeedbackController(FeedbackRepositry _comrep, JwtHandler _jwthand)
        {
            comrep = _comrep;
            jwtHandler = _jwthand;
        }



        [HttpGet("GetFeedbackQuestionsAsync")]
        public async Task<dynamic> GetFeedbackQuestionsAsync(string lngId=null)
        {

            var resp = await comrep.GetFeedbackQuestionsAndAnswersOPtion(lngId);
            return resp;

        }


        [HttpGet("GetFeedbackAnswersAsync")]
        public async Task<dynamic> GetFeedbackAnswersAsync(string lngId, int strId)
        {

            var resp = await comrep.GetFeedbackAnswersAsync(lngId, strId);
            return resp;

        }


        [HttpPost("InsertFeedbackAnswersAsync")]
        public async Task<dynamic> InsertFeedbackAnswersAsync(List<FeedbackSubmission> submissions)
        {

            var resp = await comrep.InsertFeedbackAnswersAsync(submissions);
            return resp;

        }


        [HttpGet("GetPatientIdByRandomKey")]
        public async Task<dynamic> GetPatientIdByRandomKey(string RandomKey)
        {

            var resp = await comrep.GetPatiIdByRandomKey(RandomKey);
            return resp;

        }

    }
}
