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
    public class UniqueController : ControllerBase
    {
        /// <summary>
        /// 取得資料庫所有 unique key
        /// </summary>
        [HttpGet]
        public IActionResult GetUnique(string Tab)
        {
            string strSql = @"select t.[name] as table_name,
                                    i.[name] as key_name,
	                                substring(column_names, 1, len(column_names)-1) as [columns],
                                    case when c.[type] = 'PK' then 'Primary key'
                                        when c.[type] = 'UQ' then 'Unique constraint'
                                        when i.[type] = 1 then 'Unique clustered index'　
                                        when i.type = 2 then 'Unique index'
                                        end as constraint_type
                                from sys.objects t
                                    left outer join sys.indexes i
                                        on t.object_id = i.object_id
                                    left outer join sys.key_constraints c
                                        on i.object_id = c.parent_object_id 
                                        and i.index_id = c.unique_index_id
                                   cross apply (select col.[name] + ', '
                                                    from sys.index_columns ic
                                                        inner join sys.columns col
                                                            on ic.object_id = col.object_id
                                                            and ic.column_id = col.column_id
                                                    where ic.object_id = t.object_id
                                                        and ic.index_id = i.index_id
                                                            order by col.column_id
                                                            for xml path ('') ) D (column_names)
                                where is_unique = 1 and t.is_ms_shipped <> 1 and (t.[name] = @Tab or @Tab is null)
                                order by t.[name]";

            var p = new DynamicParameters();
            p.Add("@Tab", Tab);

            using (var db = new AppDb())
            {
                List<Unique> data = db.Connection.Query<Unique>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
    }
}
