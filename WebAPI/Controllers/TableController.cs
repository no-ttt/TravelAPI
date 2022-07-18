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
    public class TableController : ControllerBase
    {
        /// <summary>
        /// 取得資料庫所有 table
        /// </summary>
        [HttpGet]
        public IActionResult GetTable()
        {
            string strSql = @"select t.name as table_name, t.create_date, t.modify_date
                                from sys.tables t
                                order by table_name;"; 

            using (var db = new AppDb())
            {
                List <Table> data = db.Connection.Query<Table>(strSql).ToList();
                return Ok(new { data });
            }
        }
    }
}
