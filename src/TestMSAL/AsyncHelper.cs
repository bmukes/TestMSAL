using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestMSAL
{
    /// <summary>
    /// https://cpratt.co/async-tips-tricks/
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
          TaskFactory(CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);
		/// <summary>
		/// Method to run an Async method synchronously
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="func"></param>
		/// <returns></returns>
		public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return AsyncHelper._myTaskFactory
              .StartNew<Task<TResult>>(func)
              .Unwrap<TResult>()
              .GetAwaiter()
              .GetResult();
        }
		/// <summary>
		/// Method to run an Async method synchronously
		/// </summary>
		/// <param name="func"></param>
		public static void RunSync(Func<Task> func)
        {
            AsyncHelper._myTaskFactory
              .StartNew<Task>(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }
}
