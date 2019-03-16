using NEL.Simple.SDK;
using NEL.Simple.SDK.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace PackLevelDbOperation
{
    public class OperationHandler
    {
        private string conn;
        private string db;
        private string coll;
        private int sleepTime;
        public UInt32 startHeight;
        public UInt32 endHeight;

        public OperationHandler(Setting setting)
        {
            conn = setting.Conn_Track;
            db = setting.DataBase_Track;
            coll = setting.Coll_Track;
            sleepTime = setting.SleepTime;
            startHeight = 0;
            endHeight = 0;
        }

        public void UploadToZip(UInt32 _startHeight, UInt32 _endHeight)
        {
            startHeight = _startHeight;
            endHeight = _endHeight;
            using (FileStream zipToOpen = new FileStream("release.zip", FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry readmeEntry = archive.CreateEntry("operation.acc");
                    using (BinaryWriter writer = new BinaryWriter(readmeEntry.Open()))
                    {
                        //先插入 开始高度已经结束高度
                        writer.Write((UInt32)startHeight);
                        writer.Write((UInt32)endHeight);
                        while (true)
                        {
                            if (startHeight > endHeight)
                                break;
                            //从mongo中获取data然后存入到本地
                            var list = MongoDBHelper.Get<MongodbOperation>(conn, db, coll, "{height:" + startHeight + "}", "{height:1}");
                            if (list.Count > 0)
                            {
                                writer.Write((UInt32)startHeight);
                                writer.Write((UInt32)list.Count);
                                for (var i = 0; i < list.Count; i++)
                                {
                                    var mo = list[i];
                                    var lo = new LevelDbOperation() { tableid = mo.tableid, key = mo?.key?.Bytes, value = mo?.value?.Bytes, state = mo.state };
                                    byte[] bytes = LevelDbOperation.Serialize(lo);
                                    writer.Write(bytes);
                                }
                            }
                            startHeight++;
                        }
                    }
                }
            }
        }

        public void GetFromZip()
        {
            using (FileStream fs = new FileStream("release.zip", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
            using (Stream zs = zip.GetEntry("operation.acc").Open())
            {
                List<LevelDbOperation> list = new List<LevelDbOperation>();
                BinaryReader b = new BinaryReader(zs);
                {
                    //开始高度
                    var startHeight = b.ReadUInt32();
                    //结束高度
                    var endHeight = b.ReadUInt32();

                    for (var i = startHeight; i <= endHeight; i++)
                    {
                        //获取到的数据的高度
                        var height = b.ReadUInt32();
                        var count = b.ReadUInt32();
                        for (var ii = 0; ii < count; ii++)
                        {
                            var l = LevelDbOperation.Deserialize(ref b);
                            list.Add(l);
                        }
                    }

                }
                b.Dispose();
            }
        }
    }
}
