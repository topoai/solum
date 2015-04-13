using solum.core.dataprocess.activities;
using solum.core.dataprocess.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.dataprocess
{
    public static class DataProcess_Builder
    {
        public static ActionActivity<T> Do<T>(this DataProcess process, Action<T> action)
        {            
            var activity = new ActionActivity<T>(process, action);

            process.AddActivity(activity);

            return activity;
        }

        /// <summary>
        /// DO : Source -> Action 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ActionActivity<T> Do<T>(this DataSource<T> source, Action<T> action)
        {
            var process = source.Process;
            var activity = new ActionActivity<T>(process, action);

            process.AddActivity(source, activity);

            return activity;
        }
        /// <summary>
        /// DO : Source -> Function
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="source"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static DataActivity<TInput, TOutput> Do<TInput, TOutput>(this DataSource<TInput> source, Func<TInput, TOutput> function)
        {
            var process = source.Process;
            var activity = new FunctionActivity<TInput, TOutput>(process, function);

            process.AddActivity(source, activity);
            return activity;
        }
        
        /// <summary>
        /// DO : Activity -> Action
        /// </summary>
        public static ActionActivity<TOut> Do<TIn, TOut>(this DataActivity<TIn, TOut> parent, Action<TOut> action)
        {
            var process = parent.Process;
            var activity = new ActionActivity<TOut>(process, action);

            process.AddActivity(parent, activity);

            return activity;
        }

        public static FunctionActivity<TOut, TResult> Do<TIn, TOut, TResult>(this DataActivity<TIn, TOut> parent, Func<TOut, TResult> function)
        {
            var process = parent.Process;
            var activity = new FunctionActivity<TOut, TResult>(process, function);

            process.AddActivity(parent, activity);

            return activity;
        }
        public static DataActivity<TOutput, TOutput> Then<TInput, TOutput>(this DataActivity<TInput, TOutput> parent, Action<TOutput> action) 
        {
            var process = parent.Process;
            var activity = new ActionActivity<TOutput>(process, action);

            process.LinkActivity(parent, activity);
            return activity;
        }
        public static DataActivity<TOutput, TResult> Then<TInput, TOutput, TResult>(this DataActivity<TInput, TOutput> parent, Func<TOutput, TResult> function)
        {
            var process = parent.Process;
            var activity = new FunctionActivity<TOutput, TResult>(process, function);

            process.LinkActivity(parent, activity);
            return activity;
        }

        public static IDataProcess Run<TIn, TOut>(this DataActivity<TIn, TOut> activity)
        {
            var process = activity.Process;
            process.Run();

            return process;
        }
    }
}
