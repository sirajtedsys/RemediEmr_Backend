using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RemediEmr.Data.Class;
using RemediEmr.Data.DbModel;
using RemediEmr.Repositry;
using static JwtService;

namespace RemediEmr.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ComplaintController : Controller
	{
		private readonly ComplaintRepository comrep;
		private readonly OPNurseRepositry nrrep;
		private readonly JwtHandler jwtHandler;

		public ComplaintController(ComplaintRepository _comrep, JwtHandler _jwthand,OPNurseRepositry _nrrep)
		{
			comrep = _comrep;
			jwtHandler = _jwthand;
			nrrep = _nrrep;
		}
		

		[HttpPost("SaveComplaint")]
		public async Task<dynamic> SaveComplaint(Complaint b)
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
					//var vit = await nrrep.ExecuteSpOnlineVitalStatusUpd(b.vital,decodedToken);
					var kk =await comrep.SaveComplaintAsync1(b, decodedToken);



					var dd = 	await comrep.SaveComplaintAsync(b, decodedToken);
					//List a = [];
					return dd;

				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in SaveComplaint: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}



		[HttpPost("ExecuteSpOnlineVitalStatusUpd")]
		public async Task<dynamic> ExecuteSpOnlineVitalStatusUpd(Vitals vt)
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
					return await comrep.ExecuteSpOnlineVitalStatusUpd(vt, decodedToken);
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in ExecuteSpOnlineVitalStatusUpd: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}



		[HttpGet("GetComplaintDetails")]
		public async Task<dynamic> GetComplaintDetails(string edocId)
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
					return await comrep.GetComplaintDetails(edocId);
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in Get Ccomplaint: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}

		[HttpGet("GetPAtientVitals")]
		public async Task<dynamic> GetPAtientVitals(string edocId)
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
					return await comrep.GetPAtientVitals(edocId);
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


		[HttpGet("GetAllIcds")]
		public async Task<dynamic> GetAllIcds(string Emrdocid)
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
					return await comrep.GetAllIcds(Emrdocid);
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in GetAllIcds: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}
		
			[HttpGet("GetActiveCaseDetails")]
		public async Task<dynamic> GetActiveCaseDetails()
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
					return await comrep.GetActiveCaseDetails();
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in GetAllCases: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}

		

			[HttpPost("SaveKnowcaseAsync")]
		public async Task<dynamic> SaveKnowcaseAsync(KnowCase b)
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

					var dd = await comrep.SaveKnowcaseAsync(b, decodedToken);
					//List a = [];
					return dd;

				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in SaveComplaint: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}


		[HttpPost("SaveSymptomsAsync")]
		public async Task<dynamic> SaveSymptomsAsync(Symptomsdet b)
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

					var dd = await comrep.SaveSymptomsAsync(b, decodedToken);
					//List a = [];
					return dd;

				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in SaveComplaint: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}

		[HttpGet("GetSymptoms")]
		public async Task<dynamic> GetSymptoms(string DoctId)
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
					return await comrep.GetSymptoms(DoctId);
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in GetAllSymtoms: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}
		
		
			[HttpGet("GetAllergy")]
		public async Task<dynamic> GetAllergy(string patId, string DoctId)
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
					return await comrep.GetAllergy(patId,DoctId);
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in GetAllSymtoms: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}
		[HttpGet("GetEduAdv")]
		public async Task<dynamic> GetEduAdv(string emrDocId)
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
					return await comrep.GetEduAdv(emrDocId);
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in GetAllSymtoms: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}

		
		[HttpGet("GetKnowcase")]
		public async Task<dynamic> GetKnowcase(string emrDocId)
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
					return await comrep.GetKnowcase(emrDocId);
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in GetAllSymtoms: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}

		
			[HttpGet("GetSymptomsDet")]
		public async Task<dynamic> GetSymptomsDet(string emrDocId)
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
					return await comrep.GetSymptomsDet(emrDocId);
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error in GetAllSymtoms: {ex.Message}");
				return StatusCode(500, "Internal server error");
			}

		}
        [HttpGet("GetBloodGroup")]
        public async Task<dynamic> GetBloodGroup()
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
                    return await comrep.GetBloodGroup();
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetAllSymtoms: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


    }
}
