using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

namespace Perigon.AspNetCore.Toolkit.Services;

/// <summary>
/// excel 操作类
/// </summary>
public class ExcelService
{
    public const string MimeType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ExcelService() { }

    /// <summary>
    /// 快捷导出
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasTitle">是否包含标题</param>
    /// <returns></returns>
    public static async Task<Stream> ExportAsync<T>(
        IEnumerable<T> data,
        string sheetName = "sheet1",
        bool hasTitle = true
    )
    {
        var stream = new MemoryStream();
        var configuration = new OpenXmlConfiguration
        {
            FastMode = true,
            EnableAutoWidth = true,
            AutoFilter = false,
            FreezeRowCount = hasTitle ? 1 : 0,
        };

        await stream.SaveAsAsync(
            data,
            printHeader: hasTitle,
            sheetName: sheetName,
            configuration: configuration
        );
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// 快捷导入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasTitle">是否包含标题</param>
    /// <returns></returns>
    public static List<T> Import<T>(Stream stream, string? sheetName = null, bool hasTitle = true)
        where T : class, new()
    {
        stream.Position = 0;
        return stream.Query<T>(sheetName: sheetName, hasHeader: hasTitle).ToList();
    }
}
