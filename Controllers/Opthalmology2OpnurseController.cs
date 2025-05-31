using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RemediEmr.Repositry;
using static JwtService;

namespace RemediEmr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Opthalmology2OpnurseController : ControllerBase
    {

        private readonly OPthalmology2OpnurseRepositry comrep;
        private readonly CommonRepositry _crep;
        private readonly JwtHandler jwtHandler;

        public Opthalmology2OpnurseController(OPthalmology2OpnurseRepositry _comrep, CommonRepositry crep, JwtHandler _jwthand)
        {
            comrep = _comrep;
            jwtHandler = _jwthand;
            _crep = crep;
        }
    }
}
