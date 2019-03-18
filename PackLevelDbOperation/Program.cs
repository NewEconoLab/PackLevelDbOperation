using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace PackLevelDbOperation
{
    class Program
    {
        static OperationHandler handler;
        static void Main(string[] args)
        {
            //初始化配置
            var setting = new Setting();
            handler = new OperationHandler(setting);
            RunConsole();
        }

        static void RunConsole()
        {
            while (true)
            {
                Console.Write($"cmd>");
                string line = Console.ReadLine()?.Trim();
                if (line == null) break;
                string[] args = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (args.Length == 0)
                    continue;
                OnCommand(args);
            }
        }

        static void OnCommand(string[] args)
        {
            switch (args[0].ToLower())
            {
                case "show":
                    OnShowComman(args);
                    break;
                case "start":
                    OnShowStart(args);
                    break;
                default:
                    break;
            }
        }

        static void OnShowStart(string[] args)
        {
            switch (args[1].ToLower())
            {
                case "upload":
                    StartUpload();
                    break;
                case "read":
                    StartRead();
                    break;
            }
        }

        static void OnShowComman(string[] args)
        {
            switch (args[1].ToLower())
            {
                case "state":
                    ShowState();
                    break;
            }
        }

        static void StartRead()
        {
            Task task1 = new Task(() =>
            {
                handler.GetFromZip();
            });

            task1.Start();
        }

        static void StartUpload()
        {
            Console.WriteLine("请输入打包的初始高度");
            var startIndex = UInt32.Parse(Console.ReadLine());
            Console.WriteLine("请输入打包的结束高度");
            var endIndex = UInt32.Parse(Console.ReadLine());
            Console.WriteLine("开始高度：" + startIndex + "结束高度：" + endIndex + "确认按回车");
            Console.ReadLine();
            Task task1 = new Task(() =>
            {
                handler.UploadToZip(startIndex, endIndex);
            });

            task1.Start();

        }

        static void ShowState()
        {
            bool stop = false;
            Task task2 = new Task(async () =>
            {
                while (!stop)
                {
                    Console.Clear();
                    Console.WriteLine(string.Format("{0}/{1}", handler?.startHeight, handler?.endHeight));
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });

            task2.Start();

            Console.ReadLine();
            stop = true;
        }
    }
}
