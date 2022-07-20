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
    public class ColumnController : ControllerBase
    {
        /// <summary>
        /// 取得資料庫所有欄位
        /// </summary>
        [HttpGet]
        public IActionResult GetColumn()
        {
            string strSql = @"select tab.name as table_name, 
                                    col.column_id,
                                    col.name as column_name, 
                                    t.name as data_type,    
                                    col.max_length,
                                    col.precision
                            from sys.tables as tab
                                    inner join sys.columns as col
                                        on tab.object_id = col.object_id
                                    left join sys.types as t
                                    on col.user_type_id = t.user_type_id
                            order by table_name, column_id;";

            using (var db = new AppDb())
            {
                List<Column> data = db.Connection.Query<Column>(strSql).ToList();
                return Ok(new { data });
            }
        }

        /// <summary>
        /// 取得指定資料表的所有欄位
        /// </summary>
        [HttpGet]
        [Route("{Tab}")]
        public IActionResult GetTabColumn(string Tab)
        {
            string strSql = @"select col.column_id as id,
                                    col.name,
                                    t.name as data_type,
                                    col.max_length,
                                    col.precision,
                                    col.is_nullable
                            from sys.tables as tab
                                    inner join sys.columns as col
                                        on tab.object_id = col.object_id
                                    left join sys.types as t
                                    on col.user_type_id = t.user_type_id
                            where tab.name = @Tab
                            order by tab.name, column_id;";

            using (var db = new AppDb())
            {
                List<TabColumn> data = db.Connection.Query<TabColumn>(strSql, new { Tab }).ToList();
                return Ok(new { tab = Tab, data });
            }
        }
    }
}
