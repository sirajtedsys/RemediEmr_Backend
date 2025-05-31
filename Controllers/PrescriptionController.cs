using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RemediEmr.Data.Class;
using RemediEmr.Repositry;
using System.Data;
using static JwtService;

namespace RemediEmr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly PrescriptionRepositry comrep;
        private readonly JwtHandler jwtHandler;

        public PrescriptionController(PrescriptionRepositry _comrep, JwtHandler _jwthand)
        {
            comrep = _comrep;
            jwtHandler = _jwthand;
        }

        [HttpGet("GetOnlineMedicineAndGeneric")]
        public async Task<dynamic> GetOnlineMedicineAndGeneric(string term, string branchId, int? claimId = null)
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
                    return await comrep.GetOnlineMedicineAndGeneric(term,branchId,decodedToken,claimId);
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

        [HttpGet("GetDosagesByEmployeeAsync")]
        public async Task<dynamic> GetDosagesByEmployeeAsync()
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
                    return await comrep.GetDosagesByEmployeeAsync(decodedToken.AUSR_ID);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetPAtientVitals: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }
       
        
        
        
        [HttpGet("GetRoutesByEmployeeAsync")]
        public async Task<dynamic> GetRoutesByEmployeeAsync()
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
                    return await comrep.GetRoutesByEmployeeAsync(decodedToken.AUSR_ID);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetRoutesByEmployeeAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }



        [HttpGet("GetFrequencyByEmployeeAsync")]
        public async Task<dynamic> GetFrequencyByEmployeeAsync()
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
                    return await comrep.GetFrequencyByEmployeeAsync(decodedToken.AUSR_ID);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetFrequencyByEmployeeAsync: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        [HttpGet("GetOnlineGenericNamesAsync")]
        public async Task<dynamic> GetOnlineGenericNamesAsync(string term, string branchId, int? claimId = 0)
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
                    return await comrep.GetOnlineGenericNamesAsync(term, branchId, claimId);
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


        [HttpPost("ExecuteOnlineDocMediIns")]
        public async Task<dynamic> ExecuteOnlineDocMediIns(List<Prescription> vt)
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
                    //int outval = 0;
                    // Return user details or appropriate response
                    //return Ok(new { Message = "User details retrieved successfully", UserDetails = decodedToken });
                     return await comrep.ExecuteSPOnlineDocMediIns(vt, decodedToken);
                    //return outval;
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in ExecuteOnlineDocMediIns: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        

              [HttpGet("GetMedicinePlanForPatient")]
        public async Task<dynamic> GetMedicinePlan(string patId, string emrDocId)
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
                    return await comrep.GetMedicinePlan(patId,emrDocId);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetMedicinePlan: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


    }
}
