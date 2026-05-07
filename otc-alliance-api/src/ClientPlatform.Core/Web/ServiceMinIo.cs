using Abp.Dependency;
using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClientPlatform.Web
{
    public class ServiceMinIo : ITransientDependency
    {

        /// <summary>
        /// 日志接口
        /// </summary>
        public ILogger _logger { get; set; }
        /// <summary>
        /// minio服务器
        /// </summary>
        public readonly IMinioClient _minio;
        /// <summary>
        /// 默认桶名 klicklpay
        /// </summary>
        public readonly string defalut_bucketName = "";
        private readonly IConfiguration _configuration;
        /// <summary>
        /// 目录
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public readonly Dictionary<int, string> dir = new Dictionary<int, string>();

        /// <summary>
        /// MinIO 对外访问的基础域名（来自 MinIO:Endpoint），用于拼接完整文件 URL
        /// </summary>
        public string PublicEndpoint { get; }


        public ServiceMinIo(IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = NullLogger.Instance;
            var endpoint = _configuration.GetSection("MinIO:Endpoint").Value;
            var accessKey = _configuration.GetSection("MinIO:AccessKey").Value;
            var secretKey = _configuration.GetSection("MinIO:SecretKey").Value;
            var withSSL = _configuration.GetSection("MinIO:WithSSL").Value == "true";
            PublicEndpoint = endpoint;
            this._minio = new MinioClient().WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .Build();
            if (withSSL)
            {
                this._minio = this._minio.WithSSL();
            }
            this.dir.Add(0, "standardbusiness");
            this.dir.Add(1, "legalcurrencyappeal");
            this.dir.Add(2, "payqrcode");
            this.dir.Add(3, "base");
            this.dir.Add(4, "user");
            this.dir.Add(5, "report");
            this.dir.Add(6, "globalPayment");
        }

        /// <summary>
        /// 拼接可直接访问的文件 URL（用于落库，前端无需二次拼接）
        /// </summary>
        public string BuildPublicFileUrl(string bucketName, string objectName)
        {
            if (string.IsNullOrWhiteSpace(bucketName) || string.IsNullOrWhiteSpace(objectName))
            {
                return string.Empty;
            }

            // objectName 可能包含前导 /，统一去掉
            var cleanObjectName = objectName.TrimStart('/');

            var withSSL = _configuration.GetSection("MinIO:WithSSL").Value == "true";
            var scheme = withSSL ? "https" : "http";
            var endpoint = PublicEndpoint ?? string.Empty;

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return $"/{bucketName}/{cleanObjectName}";
            }

            // 兼容 endpoint 已包含 http/https 的情况
            var normalizedBase = endpoint.TrimEnd('/');
            if (normalizedBase.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || normalizedBase.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return $"{normalizedBase}/{bucketName}/{cleanObjectName}";
            }

            return $"{scheme}://{normalizedBase}/{bucketName}/{cleanObjectName}";
        }


        /// <summary>
        /// 文件上传，并返回http地址
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="bucketName">桶名</param>
        /// <param name="objectName">对象名 : /test/test.png</param>
        /// <param name="contentType">http内容类型</param>
        /// <returns></returns>
        public async Task UploadFile(Stream data, string bucketName, string objectName, string contentType = "image/jpeg")
        {
            try
            {
                var bktExistArgs = new BucketExistsArgs().WithBucket(bucketName);
                bool found = await _minio.BucketExistsAsync(bktExistArgs);

                if (!found)
                {
                    var mkBktArgs = new MakeBucketArgs().WithBucket(bucketName);
                    await _minio.MakeBucketAsync(mkBktArgs);
                }

                PutObjectArgs putObjectArgs = new PutObjectArgs()
                                                 .WithBucket(bucketName)
                                                 .WithObject(objectName)
                                                 .WithStreamData(data)
                                                 .WithObjectSize(data.Length)
                                                 .WithContentType(contentType);

                await _minio.PutObjectAsync(putObjectArgs);
                //PresignedGetObjectArgs presignedPutObjectArgs = new PresignedGetObjectArgs()
                //   .WithBucket(bucketName)
                //   .WithObject(objectName)
                //   .WithExpiry(60400);
                //var url = await _minio.PresignedGetObjectAsync(presignedPutObjectArgs);
            }
            catch (System.Exception ex)
            {
                this._logger.Error($"minio上传文件失败{ex.Message}", ex);
            }
        }


        /// <summary>
        /// 文件上传，并返回http地址
        /// </summary>
        /// <param name="filePath">数据</param>
        /// <param name="bucketName">桶名</param>
        /// <param name="objectName">对象名 : /test/test.png</param>
        /// <param name="contentType">http内容类型</param>
        /// <returns></returns>
        public async Task UploadFile(string filePath, string bucketName, string objectName, string contentType)
        {
            try
            {
                var bktExistArgs = new BucketExistsArgs().WithBucket(bucketName);
                bool found = await _minio.BucketExistsAsync(bktExistArgs);

                if (!found)
                {
                    var mkBktArgs = new MakeBucketArgs().WithBucket(bucketName);
                    await _minio.MakeBucketAsync(mkBktArgs);
                }

                PutObjectArgs putObjectArgs = new PutObjectArgs()
                                                 .WithBucket(bucketName)
                                                 .WithObject(objectName)
                                                 .WithFileName(filePath)
                                                 .WithContentType(contentType);

                await _minio.PutObjectAsync(putObjectArgs);
            }
            catch (System.Exception ex)
            {
                this._logger.Error($"minio上传文件失败{ex.Message}");
            }
        }

        /// <summary>
        /// 获取文件对象流
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public async Task<Stream> GetFileObject(string bucketName, string objectName)
        {
            var ms = new MemoryStream();
            try
            {
                var args = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream((stream) =>
                    {
                        stream.CopyTo(ms);
                    });

                await _minio.GetObjectAsync(args);
                ms.Position = 0;
                return ms;
            }
            catch (Exception ex)
            {
                _logger.Error($"MinIO GetFileObject error: {ex.Message}");
                // return null or throw?
                return null;
            }
        }

        /// <summary>
        /// 获取新的对象名
        /// </summary>
        /// <param name="extension">后缀</param>
        /// <param name="dic_type">类型:0:standardbusiness,</param>
        /// <returns>key:objectName,value:url</returns>
        public KeyValuePair<string, string> GetNewObjectName(string extension, int dic_type, bool isdate)
        {
            //string dictionary = this.dir[dic_type];
            //dictionary = dictionary + "/";
            //if (isdate)
            //{
            //    dictionary += $"{Guid.NewGuid().ToString().Replace("-", "")}/";
            //}
            //dictionary = dictionary + DateTime.UtcNow.ToString("yyyyMMddHHmmsssss") + new Random().Next(0, 999999).ToString().PadLeft(6, '0') + extension;
            //return new KeyValuePair<string, string>(dictionary, "/" + defalut_bucketName + "/" + dictionary);

            string dictionary = this.dir[dic_type];
            dictionary = dictionary + "/";
            if (isdate)
            {
                dictionary += $"{Guid.NewGuid().ToString().Replace("-", "")}/";
            }
            dictionary = dictionary + Guid.NewGuid().ToString().Replace("-", "") + extension;
            return new KeyValuePair<string, string>(dictionary, "/" + defalut_bucketName + "/" + dictionary);
        }
    }
}
