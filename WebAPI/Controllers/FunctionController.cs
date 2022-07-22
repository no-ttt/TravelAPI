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
    public class FunctionController : ControllerBase
    {
        /// <summary>
        /// 取得資料庫所有 Function
        /// </summary>
        [HttpGet]
        public IActionResult GetFunction()
        {
            string strSql = @"select obj.name as name,
	                                   obj.create_date as created,
	                                   obj.modify_date as last_modified
                                from sys.objects obj
                                join sys.sql_modules mod
                                     on mod.object_id = obj.object_id
                                cross apply (select p.name + ' ' + TYPE_NAME(p.user_type_id) + ', ' 
                                             from sys.parameters p
                                             where p.object_id = obj.object_id 
                                                   and p.parameter_id != 0 
                                            for xml path ('') ) par (parameters)
                                left join sys.parameters ret
                                          on obj.object_id = ret.object_id
                                          and ret.parameter_id = 0
                                where obj.type in ('FN', 'TF', 'IF')
                                order by name";

            using (var db = new AppDb())
            {
                List<Table> data = db.Connection.Query<Table>(strSql).ToList();
                return Ok(new { data });
            }
        }
    }
}
