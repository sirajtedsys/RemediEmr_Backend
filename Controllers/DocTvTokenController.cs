using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RemediEmr.Repositry;
using static JwtService;

namespace RemediEmr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocTvTokenController : ControllerBase
    {

        private readonly DoctorTvTokenRepositry comrep;
        private readonly JwtHandler jwtHandler;

        protected readonly IConfiguration _configuration;

        public DocTvTokenController(DoctorTvTokenRepositry _comrep, JwtHandler _jwthand, IConfiguration configuration)
        {
            comrep = _comrep;
            jwtHandler = _jwthand;
            _configuration = configuration;
            _photoFolderPath = _configuration["DoctorImageUrl"];
            _adsFolderPath = _configuration["AdsUrl"];
        }
        private readonly string _photoFolderPath; // Path to your photos folder   _configuration["SectionId"];_photoFolderPath
        private readonly string _adsFolderPath;



        [HttpGet("GetDoctorRoomInfoDataTableAsync")]
        public async Task<dynamic> GetDoctorRoomInfoDataTableAsync(string deviceId)
        {

            var resp = await comrep.GetDoctorRoomInfoDataTableAsync(deviceId);
            return resp;

        }

        

             [HttpGet("MarkTokenAsReadAsync")]
        public async Task<dynamic> MarkTokenAsReadAsync(string opvdtlsId, string deviceId)
        {

            var resp = await comrep.MarkTokenAsReadAsync(opvdtlsId,deviceId);
            return resp;

        }

        //[HttpGet("GetTokenPatientInfoDataTableAsync")]
        //public async Task<dynamic> GetTokenPatientInfoDataTableAsync(string deviceId)
        //{

        //    var resp = await comrep.GetTokenPatientInfoDataTableAsync(deviceId);
        //    return resp;

        //}
    }
}
