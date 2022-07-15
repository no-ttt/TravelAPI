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
    public class PKController : ControllerBase
    {
        /// <summary>
        /// 取得資料庫所有或指定 table 的 primary key
        /// </summary>
        [HttpGet]
        public IActionResult GetPK(string Tab)
        {
            string strSql = @"select pk.[name] as pk_name,
                                    substring(column_names, 1, len(column_names)-1) as [columns],
                                    tab.[name] as table_name
                                from sys.tables tab
                                    inner join sys.indexes pk
                                        on tab.object_id = pk.object_id 
                                        and pk.is_primary_key = 1
                                   cross apply (select col.[name] + ', '
                                                    from sys.index_columns ic
                                                        inner join sys.columns col
                                                            on ic.object_id = col.object_id
                                                            and ic.column_id = col.column_id
                                                    where ic.object_id = tab.object_id
                                                        and ic.index_id = pk.index_id
                                                            order by col.column_id
                                                            for xml path ('') ) D (column_names)
                                where tab.[name] = @Tab or @Tab is null
                                order by pk.[name]";

            var p = new DynamicParameters();
            p.Add("@Tab", Tab);

            using (var db = new AppDb())
            {
                List<PK> data = db.Connection.Query<PK>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
    }
}
