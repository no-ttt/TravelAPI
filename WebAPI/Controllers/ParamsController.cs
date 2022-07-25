using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Lib;
using WebAPI.model;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParamsController : ControllerBase
    {
        /// <summary>
        /// 指定 Function / Procedure 的 Input / Output
        /// </summary>
        [HttpGet]
        public IActionResult GetFunctionIO(string name)
        {
            string strSql = @"select PARAMETER_MODE as mode,
	                                PARAMETER_NAME as name,
	                                DATA_TYPE as data_type
                                from INFORMATION_SCHEMA.PARAMETERS 
                                where SPECIFIC_NAME = @name";

            var p = new DynamicParameters();
            p.Add("@name", name);

            using (var db = new AppDb())
            {
                List<Params> data = db.Connection.Query<Params>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
    }
}
