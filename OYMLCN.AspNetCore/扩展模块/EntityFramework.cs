using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace OYMLCN
{
    /// <summary>
    /// EntityFrameworkExtensions
    /// </summary>
    public static class EntityFrameworkExtensions
    {
        /// <summary>
        /// 将符合条件的第一条数据标记为删除
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="source"></param>
        /// <param name="context"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TContext RemoveOne<TSource, TContext>(this DbSet<TSource> source, TContext context, Expression<Func<TSource, bool>> predicate) where TSource : class where TContext : DbContext
        {
            var item = source.FirstOrDefault(predicate);
            if (item != null)
                context.Remove(item);
            return context;
        }
        /// <summary>
        /// 将符合条件的第一条数据删除并保存到源数据
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="source"></param>
        /// <param name="context"></param>
        /// <param name="predicate"></param>
        /// <returns>成功删除时返回True</returns>
        public static bool RemoveOneAndSave<TSource, TContext>(this DbSet<TSource> source, TContext context, Expression<Func<TSource, bool>> predicate) where TSource : class where TContext : DbContext
        {
            return source.RemoveOne(context, predicate).SaveChanges() > 0;
        }
    }
}
