﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _008异步操作中异常处理
{
    class Program
    {
        static void Main(string[] args)
        {
            //NewMethod1();

            // NewMethod2();

            //Task task = NewMethod3();
            //Console.WriteLine(task.IsFaulted);//注意IsFaulted为false，因为抛出的异常被捕获

            NewMethod4();

            Console.ReadKey();
        }


        //演示1：无法捕获异常
        //多打断点,就可以发现为何捕捉不到异常了，
        //因为执行到ThrowEx(2000, "异常信息")，开始异步方法中的await表达式，
        //即创建一个新的线程，在后台执行await表达式
        //主线程中继续ThrowEx(2000, "异常信息"); 后的代码，此时，异步方法中还在等待await表达式的执行，还没有抛出我们自己定义的异常
        // 所以此时就没有异常抛出，所以catch语句也就捕获不到异常
        // 而当异步方法抛出异常，此时主线程中catch语句已经执行完毕了！
        private static void NewMethod1()
        {
            try
            {
                ThrowEx("异常信息");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey(); Console.ReadKey();
        }


        //演示2：捕获异常
        private static async void NewMethod2()
        {
            try
            {
                //Task t = ThrowEx("这是异常信息");
                //t.Wait();//通过阻塞线程，我们也是可以捕获异常，但是你会发现并不是简单的我们抛出的异常
                //他捕获的异常的信息是“有一个或多个异常”

                await ThrowEx("这是异常信息");

                //Console.WriteLine("111111111111111111111111111");
                //undone:为什么这一句没有运行
                //看上去好像"await ThrowEx("这是异常信息");"这一句并不是异步操作，因为在等待，而不是执行主线程的后续代码
                //其实你要想一想，我们在调用异步方法的时候是不需要使用await关键字的，但是这里为捕获异常使用了await关键字
                //其实效果上和使用
            }
            catch (Exception ex)
            {
                Console.WriteLine($"捕获到异常：{ex.Message}");
            }
            Console.ReadKey();
        }

        private static async Task ThrowEx(string message, int ms = 3000)
        {
            //await Task.Delay(ms).ContinueWith(t => { Console.WriteLine("hello world");  });
            await Task.Run(() => { Thread.Sleep(ms); Console.WriteLine($"当前的线程Id：{Thread.CurrentThread.ManagedThreadId,2}:hello world"); });
            throw new Exception(message);
        }

        //演示3：将异步方法语句写在try catch中
        private static async Task NewMethod3()
        {
            try
            {
                await Task.Run(() => { Thread.Sleep(2000); Console.WriteLine("hello world"); });
                throw new Exception("这是异常信息");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //演示4：多个异步方法的异常处理
        //使用WhenAll或WhenAny异步的等待异步结果，
        //其中WhenAll就是WaitAll的异步版本，WhenAny就是WaitAny的异步版本
        private static async Task NewMethod4()
        {
            Task taskResult = null;//注意因为在catch语句中需要使用这个WhenAll的返回值，所以定义在try语句之外。
            try
            {
                Console.WriteLine($"当前的线程Id：{Thread.CurrentThread.ManagedThreadId,2}:do something before task");
                Task t1 = ThrowEx($"这是第一个抛出的异常信息:异常所在线程ID：{Thread.CurrentThread.ManagedThreadId,2}", 3000);
                Task t2 = ThrowEx($"这是第二个抛出的异常信息:异常所在线程ID：{Thread.CurrentThread.ManagedThreadId,2}", 5000);

                await (taskResult = Task.WhenAll(t1, t2));
                //await Task.WhenAll(t1, t2);//注意这样抛出的异常则不是AggregateException类型的异常
                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine($"当前的线程Id：{Thread.CurrentThread.ManagedThreadId,2}:当前循环次数{i}");
                }

            }
            catch (Exception )//注意这里捕获的异常只是WhenAll()等待的异步任务中第一抛出的异常
            {
                foreach (var item in taskResult.Exception.InnerExceptions)//通过WhenAll()的返回对象的Exception属性来查阅所有的异常信息
                //注意这个taskResult中的Exception是AggregateException类型的异常
                //该异常中不仅有InnerExcption属性还有InnerExcptions属性，InnerExceptions属性则包含所有异常
                {
                    Console.WriteLine($"当前的线程Id：{Thread.CurrentThread.ManagedThreadId,2}:{item.Message}");
                }
            }

        }
    }
}
