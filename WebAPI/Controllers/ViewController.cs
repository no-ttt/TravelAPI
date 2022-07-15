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
    public class ViewController : ControllerBase
    {
        /// <summary>
        /// 取得資料庫所有 view table
        /// </summary>
        [HttpGet]
        public IActionResult GetView()
        {
            string strSql = @"select v.name as view_name,
                                    v.create_date as created,
                                    v.modify_date as last_modified,
                                    m.definition
                                from sys.views v
                                join sys.sql_modules m 
                                    on m.object_id = v.object_id
                                order by view_name;";

            using (var db = new AppDb())
            {
                List<View> data = db.Connection.Query<View>(strSql).ToList();
                return Ok(new { data });
            }
        }

        /// <summary>
        /// view table 的相關 table
        /// </summary>
        [HttpGet]
        [Route("RelatedTab")]
        public IActionResult GetTable()
        {
            string strSql = @"select distinct v.name as view_name,
                                    o.name as referenced_entity_name,
                                    o.type_desc as entity_type
                                from sys.views v
                                join sys.sql_expression_dependencies d
                                    on d.referencing_id = v.object_id
                                    and d.referenced_id is not null
                                join sys.objects o
                                    on o.object_id = d.referenced_id
                                order by view_name;";

            using (var db = new AppDb())
            {
                List<ViewRelatedTable> data = db.Connection.Query<ViewRelatedTable>(strSql).ToList();
                return Ok(new { data });
            }
        }

        /// <summary>
        /// 所有或指定 view table 的欄位
        /// </summary>
        [HttpGet]
        [Route("Column")]
        public IActionResult GetColumn(string Tab)
        {
            string strSql = @"select object_name(c.object_id) as view_name,
                                    c.column_id,
                                    c.name as column_name,
                                    type_name(user_type_id) as data_type,
                                    c.max_length,
                                    c.precision
                                from sys.columns c
                                join sys.views v 
                                    on v.object_id = c.object_id
                                where object_name(c.object_id) = @Tab or @Tab is null
                                order by view_name, column_id;";

            var p = new DynamicParameters();
            p.Add("@Tab", Tab);
             
            using (var db = new AppDb())
            {
                List<ViewColumn> data = db.Connection.Query<ViewColumn>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
    }
}
