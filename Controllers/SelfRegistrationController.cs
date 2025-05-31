using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using RemediEmr.Data.Class;
using RemediEmr.Repositry;
using static JwtService;

namespace RemediEmr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelfRegistrationController : ControllerBase
    {

        private readonly SelfRegistrationRepositry comrep;
        private readonly JwtHandler jwtHandler;

        public SelfRegistrationController(SelfRegistrationRepositry _comrep, JwtHandler _jwthand)
        {
            comrep = _comrep;
            jwtHandler = _jwthand;
        }

      
        [HttpGet("VerifyMobileNumber")]
        public async Task<dynamic> VerifyMobileNumber(string mob)
        {
            try
            {
               
                 

             
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetPatientDetailsByMobileAsync(mob);
             
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in VerifyMobileNumber: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpGet("LookupLocationAsync")]
        public async Task<dynamic> LookupLocationAsync()
        {
            try
            {
               
               
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.LookupLocationAsync();
              
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in LookupLocationAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }



        [HttpGet("LookupPatientTypeAsync")]
        public async Task<dynamic> LookupPatientTypeAsync()
        {
            try
            {
               
                 

                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetPatientTypesAsync();
             
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in LookupPatientTypeAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        [HttpGet("GetDefaultCampCustAsync")]
        public async Task<dynamic> GetDefaultCampCustAsync()
        {
            try
            {
             

             
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetDefaultCampCustAsync();
             
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetDefaultCampCustAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        [HttpGet("LookupNationalityAsync")]
        public async Task<dynamic> LookupNationalityAsync()
        {
            try
            {

                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.LookupNationalityAsync();
             
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in LookupNationalityAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }



        [HttpGet("GetDefaultPatientTypeAsync")]
        public async Task<dynamic> GetDefaultPatientTypeAsync()
        {
            try
            {
               
                 

               
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetDefaultPatientTypeAsync();
                    //return Ok(new { value = d });
                    //return Ok(new { data = d });
               
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetDefaultPatientTypeAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        [HttpGet("GetLocationMandatorySettingAsync")]
        public async Task<dynamic> GetLocationMandatorySettingAsync()
        {
            try
            {
               
                 

                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    var d = await comrep.GetLocationMandatorySettingAsync();
                    return Ok(new { value = d });
            
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetLocationMandatorySettingAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }



        [HttpGet("GetDefaultNationalityAsync")]
        public async Task<dynamic> GetDefaultNationalityAsync()
        {
            try
            {
               

                
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetDefaultNationalityAsync();
             
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetDefaultNationalityAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        [HttpGet("LookupSalutationAsync")]
        public async Task<dynamic> LookupSalutationAsync()
        {
            try
            {
               
                 

                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.LookupSalutationAsync();
             
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in UserDetails: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        [HttpGet("GetCampPatientAsync")]
        public async Task<dynamic> GetCampPatientAsync(string branchCode, string patientTypeId, string year)
        {
            try
            {
               
                 

                
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetCampPatientAsync(branchCode, patientTypeId, year);
              
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in UserDetails: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        [HttpGet("LookupCustomerAsync")]
        public async Task<dynamic> LookupCustomerAsync(string pId, string branchCode)
        {
            try
            {
               
                 

                //if (string.IsNullOrEmpty(authorizationHeader))
                //{
                //    return Unauthorized();
                //}

                //// Extract token from header (remove "Bearer " prefix)
                //string token = authorizationHeader.Replace("Bearer ", "");

                //// Decode token (not decrypt, assuming DecriptTocken is for decoding)
                //UserTocken decodedToken = jwtHandler.DecriptTocken(authorizationHeader);

                //if (decodedToken == null)
                //{
                //    return Unauthorized();
                //}

                //// Validate token
                //var isValid = await jwtHandler.ValidateToken(token);

                //if (isValid)
                //{
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.LookupCustomerAsync(pId, branchCode);
                //}
                //else
                //{
                //    return Unauthorized();
                //}
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in LookupCustomerAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        #region Selfregistration
        [HttpPost("UpdateDailyTokenDetails")]
        public async Task<dynamic> UpdateDailyTokenDetails(DailyToken pt)
        {
            try
            {



                //   if (string.IsNullOrEmpty(authorizationHeader))
                //   {
                //       return Unauthorized();
                //   }

                //   // Extract token from header (remove "Bearer " prefix)
                //isValid)
                //   {
                // Return user details or appropriate response
                //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                return await comrep.UpdateDailyTokenDetails(pt, pt.TokenId);
                //}
                //else
                //{
                //    return Unauthorized();
                //}
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in UpdateDailyTokenDetails: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpGet("GetIdentityCards")]
        public async Task<dynamic> GetIdentityCards()
        {
            try
            {
               
                 

               
                 
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetIdentityCards();
               
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetIdentityCards: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        #endregion Selfregistration




               [HttpGet("GetPriorityTokensAsync")]
        public async Task<dynamic>GetPriorityTokensAsync(string strDeviceId)
        {
            try
            {


                return await comrep.GetPriorityTokensAsync(strDeviceId);
             
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetPriorityTokensAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        [HttpGet("SaveDailyToken")]
        public async Task<dynamic> SaveDailyToken(string tokenSeries, string deviceId)
        {
            try
            {


                return await comrep.SaveDailyToken(tokenSeries, deviceId);

            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in SaveDailyToken: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "kiosk");

        [HttpGet("{fileName}")]
        public IActionResult GetFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest("File name is required.");
            }

            // Combine with storage path
            var filePath = Path.Combine(_storagePath, fileName);

            // Check if file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            // Determine the MIME type
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(filePath, contentType);
        }



    }
}
