using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace TinyStore.Site
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                // .AddNewtonsoftJson(options =>
                // {
                //     Global.Json.Settings.JsonSerializerSettingsAction(options.SerializerSettings);
                // })
                .AddJsonOptions(options =>
                {
                    //序列化输出内容不更改属性大小写
                    //options.JsonSerializerOptions.PropertyNamingPolicy = null;
                })
                .AddRazorRuntimeCompilation();

            #region 添加 压缩规则和支持类型

            //一定记得在Configure方法中调用 app.UseResponseCompression(); 开启并使用压缩规则
            services.AddResponseCompression(options =>
                {
                    //https请求下是否开启压缩
                    options.EnableForHttps = true;
                    //添加Brotli压缩支持
                    options.Providers.Add<BrotliCompressionProvider>();
                    //配置Gzip压缩支持
                    options.Providers.Add<GzipCompressionProvider>();
                    //针对哪些下发数据类型开启压缩 
                    //ResponseCompressionDefaults.MimeTypes  常见需要开启压缩的数据类型
                    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                    {
                        "application/xhtml+xml",
                        "application/atom+xml",
                        "image/*",
                        "video/*",
                    });
                })
                //配置Brotli压缩等级
                .Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; })
                //配置Gzip压缩等级
                .Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });

            #endregion

            #region 添加缓存规则

            services.AddHttpCacheHeaders(
                //过期规则(内存缓存)
                expires =>
                {
                    //内存缓存时间(秒数)
                    expires.MaxAge = 180;
                    //允许哪些位置可以内存缓存 Public=所有(代理服务器,cdn加速服务器,客户端浏览器) Private=客户端浏览器
                    expires.CacheLocation = Marvin.Cache.Headers.CacheLocation.Public;
                },
                //验证规则(标签比较判断)
                validation =>
                {
                    //响应过期 重启验证(下发header带last-modified和etag标签,用以后期判断是否过期)
                    validation.MustRevalidate = true;
                }
            );

            #endregion

            #region 添加 路由规则

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true; //网址小写 
                options.AppendTrailingSlash = false; //添加反斜杠
            });

            #endregion

            //注册全局HttpContext并缓存全局Configuration
            SiteContext.ConfigurationService(services, Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            #region 开启并使用压缩规则

            app.UseResponseCompression();

            #endregion

            #region 开启并使用http缓存

            //在 app.UseRouting();app.UseAuthorization();之前
            app.UseHttpCacheHeaders();

            #endregion

            #region 开启并使用默认静态文件访问

            //默认允许访问目录为wwwroot 默认映射访问路径为/ 
            app.UseStaticFiles();

            //再次设配置默认配置,必须调用完app.UseStaticFiles();然后再调用下面的方法,不然会出现错误
            //且FileProvider和RequestPath不能设置,保证配置的ContentTypeProvider会对应到默认映射上
            app.UseStaticFiles(new StaticFileOptions()
            {
                //通过配置ContentTypeProvider设置Mime和文件后缀的对应关系,方便一些特殊类型的文件可以被访问
                //下面设置允许访问apk,ipa,plist和nupkg类型的文件
                ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
                {
                    {".apk", "application/vnd.android.package-archive"},
                    {".ipa", "application/iphone"},
                    {".plist", "application/xml"},
                }),
                HttpsCompression = Microsoft.AspNetCore.Http.Features.HttpsCompressionMode.Compress,
            });

            #endregion

            #region 开启并使用自定义静态文件访问

            app.UseStaticFiles(new StaticFileOptions()
            {
                //通过配置FileProvider和RequestPath实现某个目录映射某个路径的访问
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "views", "home")),
                RequestPath = "/themes",

                //通过配置ServeUnknownFileTypes 为 true 可以允许所有未知类型的文件，一般不建议这么设置,安全性较低
                //ServeUnknownFileTypes = true,

                //通过配置ContentTypeProvider设置Mime和文件后缀的对应关系,方便一些特殊类型的文件可以被访问
                //下面设置允许访问图片类型的文件
                ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
                {
                    //{ ".apk","application/vnd.android.package-archive"},
                    //{ ".ipa","application/iphone"},
                    //{ ".plist","application/xml"},
                    {".otf", "application/octet-stream"},
                    {".eot", "application/octet-stream"},
                    {".woff", "application/octet-stream"},
                    {".ttf", "application/octet-stream"},
                    {".svg", "image/svg+xml"},
                    {".css", "text/css"},
                    {".png", "image/png"},
                    {".jpg", "image/jpeg"},
                    {".gif", "image/gif"},
                    {".bmp", "image/bmp"}
                }),

                HttpsCompression = Microsoft.AspNetCore.Http.Features.HttpsCompressionMode.Compress,
            });

            #endregion


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}");
            });
        }
    }
}