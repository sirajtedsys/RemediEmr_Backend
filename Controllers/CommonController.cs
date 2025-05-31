using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RemediEmr.Data.Class;
using RemediEmr.Repositry;
using static JwtService;
using System.Runtime.ConstrainedExecution;

namespace RemediEmr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly CommonRepositry comrep;
        private readonly JwtHandler jwtHandler;

        public CommonController(CommonRepositry _comrep, JwtHandler _jwthand)
        {
            comrep = _comrep;
            jwtHandler = _jwthand;
        }


        [HttpPost("CheckLogin")]
        public async Task<dynamic> LoginCheck(Login log)
        {
            if (log != null)
            {
                var resp = await comrep.LoginCheck(log.Username,log.Password);
                return resp;
            }
            else
            {
                return BadRequest();
            }
         }

        [HttpGet("GetSectionId")]
        public async Task<dynamic> GetSectionId()
        {
            
                var resp = await comrep.GetSectionId();
                return resp;
         
        }


        [HttpGet("GetLocations")]
        public async Task<dynamic> GetActiveDisplayLocations()
        {

            var resp = await comrep.GetActiveDisplayLocations();
            return resp;

        }



        [HttpGet("GetAllUserBranches")]
        public async Task<dynamic> GetAllUserBranches()
        {
            try
            {
                // Retrieve token from Authorization header
                string authorizationHeader = Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return Unauthorized();
                }

                // Extract token from header (remove "Bearer " prefix)
                string token = authorizationHeader.Replace("Bearer ", "");

                // Decode token (not decrypt, assuming DecriptTocken is for decoding)
                UserTocken decodedToken = jwtHandler.DecriptTocken(authorizationHeader);

                if (decodedToken == null)
                {
                    return Unauthorized();
                }

                // Validate token
                var isValid = await jwtHandler.ValidateToken(token);

                if (isValid)
                {
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetAllUserBranches(decodedToken);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in UserDetails: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpPost("CheckLogin1")]
        public async Task<dynamic> LoginCheck1(Login log)
        {
            if (log != null)
            {
                var resp = await comrep.LoginCheck1(log.Username, log.Password);
                return resp;
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpGet("GetMenuDetails")]
        public async Task<dynamic> GetMenuDetails()
        {
            try
            {
                // Retrieve token from Authorization header
                string authorizationHeader = Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return Unauthorized();
                }

                // Extract token from header (remove "Bearer " prefix)
                string token = authorizationHeader.Replace("Bearer ", "");

                // Decode token (not decrypt, assuming DecriptTocken is for decoding)
                UserTocken decodedToken = jwtHandler.DecriptTocken(authorizationHeader);

                if (decodedToken == null)
                {
                    return Unauthorized();
                }

                // Validate token
                var isValid = await jwtHandler.ValidateToken(token);

                if (isValid)
                {
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetMenuDetails(decodedToken);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the GetUserDet
                Console.WriteLine($"Error in GetUserDet: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        [HttpGet("GetUserDet")]
        public async Task<dynamic> GetUserDet()
        {
            try
            {
                // Retrieve token from Authorization header
                string authorizationHeader = Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return Unauthorized();
                }

                // Extract token from header (remove "Bearer " prefix)
                string token = authorizationHeader.Replace("Bearer ", "");

                // Decode token (not decrypt, assuming DecriptTocken is for decoding)
                UserTocken decodedToken = jwtHandler.DecriptTocken(authorizationHeader);

                if (decodedToken == null)
                {
                    return Unauthorized();
                }

                // Validate token
                var isValid = await jwtHandler.ValidateToken(token);

                if (isValid)
                {
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetUserDet(decodedToken);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the GetUserDet
                Console.WriteLine($"Error in GetUserDet: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        //userwanted and allowed repotrs
        [HttpGet("GetAppMenuAsync")]
        public async Task<dynamic> GetAppMenuAsync()
        {
            try
            {
                // Retrieve token from Authorization header
                string authorizationHeader = Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return Unauthorized();
                }

                // Extract token from header (remove "Bearer " prefix)
                string token = authorizationHeader.Replace("Bearer ", "");

                // Decode token (not decrypt, assuming DecriptTocken is for decoding)
                UserTocken decodedToken = jwtHandler.DecriptTocken(authorizationHeader);

                if (decodedToken == null)
                {
                    return Unauthorized();
                }

                // Validate token
                var isValid = await jwtHandler.ValidateToken(token);

                if (isValid)
                {
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetAppMenuAsync(decodedToken.AUSR_ID);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetAppMenuAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }



        //userwanted and allowed repotrs
        [HttpGet("GetPatientDetailsAsync")]
        public async Task<dynamic> GetPatientDetailsAsync(string patiId, string pEdocId = null, string edocId = null)
        {
            try
            {
                // Retrieve token from Authorization header
                string authorizationHeader = Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return Unauthorized();
                }

                // Extract token from header (remove "Bearer " prefix)
                string token = authorizationHeader.Replace("Bearer ", "");

                // Decode token (not decrypt, assuming DecriptTocken is for decoding)
                UserTocken decodedToken = jwtHandler.DecriptTocken(authorizationHeader);

                if (decodedToken == null)
                {
                    return Unauthorized();
                }

                // Validate token
                var isValid = await jwtHandler.ValidateToken(token);

                if (isValid)
                {
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetPatientDetailsAsyncssss(patiId, pEdocId, edocId);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetAppMenuAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpGet("GetDentalAssessmentAsync")]
        public async Task<dynamic> GetDentalAssessmentAsync(string edocId)
        {
            try
            {
                // Retrieve token from Authorization header
                string authorizationHeader = Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return Unauthorized();
                }

                // Extract token from header (remove "Bearer " prefix)
                string token = authorizationHeader.Replace("Bearer ", "");

                // Decode token (not decrypt, assuming DecriptTocken is for decoding)
                UserTocken decodedToken = jwtHandler.DecriptTocken(authorizationHeader);

                if (decodedToken == null)
                {
                    return Unauthorized();
                }

                // Validate token
                var isValid = await jwtHandler.ValidateToken(token);

                if (isValid)
                {
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetDentalAssessmentAsync(edocId);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetAppMenuAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }




        [HttpPost("InsertDentalAssessment")]
        public async Task<dynamic> InsertDentalAssessment(DentalAssessmentModel dentalAssessment)
        {
            try
            {
                // Retrieve token from Authorization header
                string authorizationHeader = Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return Unauthorized();
                }

                // Extract token from header (remove "Bearer " prefix)
                string token = authorizationHeader.Replace("Bearer ", "");

                // Decode token (not decrypt, assuming DecriptTocken is for decoding)
                UserTocken decodedToken = jwtHandler.DecriptTocken(authorizationHeader);

                if (decodedToken == null)
                {
                    return Unauthorized();
                }

                // Validate token
                var isValid = await jwtHandler.ValidateToken(token);

                if (isValid)
                {
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.SaveDentalAssessmentAsync(dentalAssessment, decodedToken);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetAppMenuAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        




        [HttpGet("GetPatientListDatewiseAsync")]
        public async Task<dynamic> GetPatientListDatewiseAsync(string empId, string sctId, string? name = null, string? mobile = null,
        string? cprNo = null, string? visitType = null, int groupId = 0, DateTime? date = null,
        string? listType = null, string? levelId = null, string branchId = null)
        {
            try
            {
                // Retrieve token from Authorization header
                string authorizationHeader = Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return Unauthorized();
                }

                // Extract token from header (remove "Bearer " prefix)
                string token = authorizationHeader.Replace("Bearer ", "");

                // Decode token (not decrypt, assuming DecriptTocken is for decoding)
                UserTocken decodedToken = jwtHandler.DecriptTocken(authorizationHeader);

                if (decodedToken == null)
                {
                    return Unauthorized();
                }

                // Validate token
                var isValid = await jwtHandler.ValidateToken(token);

                if (isValid)
                {
                    // Return user details or appropriate response
                    //return Ok(new { Message = "CallCollectionProcedureAsync details retrieved successfully", UserDetails = decodedToken });
                    return await comrep.GetPatientListDatewiseAsync(empId, sctId, name, mobile,
                        cprNo, visitType, groupId, date, listType, levelId, branchId);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetPatientListDatewiseAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        [HttpGet("SaveSpecialConsultation")]
        public async Task<dynamic> SaveSpecialConsultation(string patiId, string edocId, string specialConsult, string pTreatmentRemarksNew)
        {
            try
            {
                // Retrieve token from Authorization header
                string authorizationHeader = Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return Unauthorized();
                }

                // Extract token from header (remove "Bearer " prefix)
                string token = authorizationHeader.Replace("Bearer ", "");

                // Decode token (not decrypt, assuming DecriptTocken is for decoding)
                UserTocken decodedToken = jwtHandler.DecriptTocken(authorizationHeader);

                if (decodedToken == null)
                {
                    return Unauthorized();
                }

                // Validate token
                var isValid = await jwtHandler.ValidateToken(token);

                if (isValid)
                {
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                    var a = await comrep.SaveCompImmuDetailsAsync(patiId, edocId, decodedToken.AUSR_ID, null, pTreatmentRemarksNew, null, null, null);
                    var b = await comrep.SaveSpecialConsultationAsync(patiId, edocId, specialConsult, decodedToken);

                    List<object> objects = new List<object>();
                    objects.Add(b);
                    objects.Add(a);
                    return objects;
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetAppMenuAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

    }


}

