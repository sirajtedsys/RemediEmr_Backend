using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using RemediEmr.Repositry;
using static JwtService;

namespace RemediEmr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TvTokenController : ControllerBase
    {
        private readonly TvTokenRepositry comrep;
        private readonly JwtHandler jwtHandler;

        protected readonly IConfiguration _configuration;

        public TvTokenController(TvTokenRepositry _comrep, JwtHandler _jwthand, IConfiguration configuration)
        {
            comrep = _comrep;
            jwtHandler = _jwthand;
            _configuration = configuration;
            _photoFolderPath = _configuration["DoctorImageUrl"];
            _adsFolderPath = _configuration["AdsUrl"];
        }
        private readonly string _photoFolderPath; // Path to your photos folder   _configuration["SectionId"];_photoFolderPath
        private readonly string _adsFolderPath;

        [HttpGet("GetPhoto")]
        //[HttpGet("GetFile")]
        public IActionResult GetPhoto(string fileName)
        {
            var filePath = Path.Combine(_photoFolderPath, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string mimeType))
            {
                mimeType = "application/octet-stream"; // Default MIME type==
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Check if the file is a video (adjust extensions as needed)
            if (mimeType.StartsWith("video/"))
            {
                return File(stream, mimeType, enableRangeProcessing: true); // ✅ Enables video streaming
            }

            return File(stream, mimeType); // Normal file response for images and other files
        }


        [HttpGet("GetPhotoOrVideoforAds")]
        public IActionResult GetPhotoOrVideoforAds(string fileName)
        {
            try
            {
                // Normalize slashes
                fileName = fileName.Replace("\\", "/");

                // Ensure no leading slash
                if (fileName.StartsWith("/"))
                {
                    fileName = fileName.Substring(1);
                }

                // Build the full path correctly
                var filePath = Path.Combine(_adsFolderPath, fileName);

                if (!System.IO.File.Exists(filePath))
                    return NotFound();

                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(filePath, out string mimeType))
                {
                    mimeType = "application/octet-stream"; // Default MIME type
                }

                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                if (mimeType.StartsWith("video/"))
                {
                    return File(stream, mimeType, enableRangeProcessing: true); // ✅ Enables video streaming
                }

                return File(stream, mimeType); // Normal file response for images
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }



        [HttpGet("GetTvTokenHomePageData")]
        public async Task<IActionResult> GetTvTokenHomePageData()
        {
            // Access the TvTokenHomePageData section dynamically
            var tvTokenHomePageData = _configuration.GetSection("TvTokenHomePageData").Get<Dictionary<string, object>>();

            // Check if the data was loaded successfully
            if (tvTokenHomePageData == null)
            {
                return NotFound("TvTokenHomePageData not found in configuration.");
            }

            // Return as JSON response
            return Ok(tvTokenHomePageData);
        }



        [HttpGet("SaveOrUpdateDevice")]
        public async Task<dynamic> SaveOrUpdateDevice(string deviceId, string fcmToken, string name, string dispLocationId)
        {

            var resp = await comrep.SaveOrUpdateDevice(deviceId, fcmToken, name, dispLocationId);
            return resp;

        }
      
        
        [HttpGet("GetTokenDetails")]
        public async Task<dynamic> GetTokenDetails(string deviceId)
        {

            var resp = await comrep.GetTokenDetails(deviceId);
            return resp;

        }
        [HttpGet("UpdateTokenReadVoiceStatusAsync")]
        public async Task<dynamic> UpdateTokenReadVoiceStatusAsync(string billHdrId, string tokenCatId)
        {

            var resp = await comrep.UpdateTokenReadVoiceStatusAsync(billHdrId,tokenCatId);
            return resp;


        }
        //UpdateTokenReadVoiceStatusAsync(int billHdrId, int tokenCatId)
        [HttpGet("GetSettings")]
        public async Task<dynamic> GetSettings()
        {

            var resp = await comrep.GetSettings();
            return resp;

        }

        [HttpGet("GetAdsAsync")]
        public async Task<dynamic> GetAdsAsync(string deviceId)
        {

            var resp = await comrep.GetAdsAsync(deviceId);
            return resp;


        }



    }
}
