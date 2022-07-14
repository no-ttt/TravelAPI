using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Lib;
using WebAPI.model;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        /// <summary>
        /// 取得資料庫所有資料表
        /// </summary>
        [HttpGet]
        public IActionResult GetTables()
        {
            string strSql = @"select t.name as table_name, t.create_date, t.modify_date
                                from sys.tables t
                                order by table_name;"; 

            using (var db = new AppDb())
            {
                List <Tables> data = db.Connection.Query<Tables>(strSql).ToList();
                return Ok(new { data });
            }
        }
    }
}
