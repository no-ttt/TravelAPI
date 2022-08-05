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
        public IActionResult GetFuncProcIO(string name)
        {
            string strSql = @"select case par.is_output
                                        when 1 then 'OUT'
                                        when 0 then 'IN'
                                    end as mode,
	                                par.name as name,
	                                t.name as data_type,
	                                ISNULL(p.value, '') as des
                                from sys.objects obj
                                inner join sys.parameters par
	                                on obj.object_id = par.object_id
                                left join sys.extended_properties p
	                                on par.object_id = p.major_id and par.parameter_id = p.minor_id
                                left join sys.types t
	                                on par.system_type_id = t.system_type_id
                                where obj.name = @name";

            var p = new DynamicParameters();
            p.Add("@name", name);

            using (var db = new AppDb())
            {
                List<Params> data = db.Connection.Query<Params>(strSql, p).ToList();
                return Ok(new { name = name, data });
            }
        }
    }
}
