using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RemediEmr.Class;
using RemediEmr.Data.DbModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
//using System.Web.Http.Results;

public class JwtService
{






    //------------------------------------------------


    //using Api.Class.ProceduralClass;
    //using Api.Model.DB;
    //using Microsoft.AspNetCore.Mvc;
    //using Microsoft.EntityFrameworkCore;
    //using Microsoft.IdentityModel.Tokens;
    //using System.ComponentModel.DataAnnotations;
    //using System.IdentityModel.Tokens.Jwt;
    //using System.Security.Claims;
    //using System.Text;

    //namespace Api.Services
    //{
    public class UserTocken
    {
        [Key]
        public string AUSR_ID { get; set; }
        public string USERNAME { get; set; }

        public string PASSWORD { get; set; }

        
    }
    public class JwtHandler
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;

        public JwtHandler(IConfiguration configuration, AppDbContext DBContext)
        {
            _configuration = configuration;
            _dbContext = DBContext;
        }


        //--------------Fucntion That Genarete Token----------------------------------------------------

        public string GenerateToken(UserTocken user)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("AuserId", user.AUSR_ID.ToString()),
            new Claim("Username", user.USERNAME),
            new Claim("Password", user.PASSWORD)
        };

                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Issuer"],
                    claims,
                    expires: DateTime.Now.AddDays(2),
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                // Log the exception
                return "An error occurred while generating the token.";
            }
        }


        //--------------Token Decript--------------------------------------------
        public UserTocken DecriptTocken(string TokenID)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.ReadToken(Convert.ToString(TokenID).Trim().Split(" ")[1]) as JwtSecurityToken;
            UserTocken usertoken = new UserTocken()
            {

                AUSR_ID = token.Claims.First(x => x.Type == "AuserId")?.Value,
                USERNAME = token.Claims.First(x => x.Type == "Username")?.Value,
                PASSWORD = token.Claims.First(x => x.Type == "Password")?.Value
            };
            return usertoken;

        }


        //----------------------Function For Validate Token------------------------------------------------

        public async Task<bool> ValidateToken(string token)
        {
            if (token == null)
                return false;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Token validation
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Issuer"],
                    IssuerSigningKey = securityKey
                }, out SecurityToken validatedToken);
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return false;
            }

            // If validation succeeded, proceed to additional checks
            try
            {

                var dat = await _dbContext.LOGIN_SETTINGS
                                           .Where(u => u.TOKEN == token).ToListAsync();
                var validatetoken = new UCHMASTER_LoginSettings();

                if(dat.Count>0)
                {
                    validatetoken = dat[0];
                }
                else
                {
                    validatetoken=null;
                }

                if (validatetoken != null)
                {
                    if(validatetoken.ID>0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    // Return the empid as Id with Error = 0 (assuming 0 means success)
                    
                }
                else
                {
                    // If no match is found, return null Id with an appropriate error code
                    return false; // Assuming 1 for some error status
                }
                // Example SQL query to validate token in database
                //var sqlQuery = $"Exec Sp_ValidateToken @Token='{token}'";
                //var result = await _dbContext.Message2.FromSqlRaw(sqlQuery).ToListAsync();

                //if (result.Count > 0)
                //{
                //    if (result[0].Id > 0)
                //    {
                //        return true;
                //    }
                //    else
                //    {

                //        return false;
                //    }

                //    // Additional logic based on the validation result
                //    // You may choose to return additional information or perform other actions
                //}
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                Console.WriteLine($"Database validation error: {ex.Message}");
                return false;
            }

            return true;
        }

        //------------------------------------------------------------------------------
    }
}

//}