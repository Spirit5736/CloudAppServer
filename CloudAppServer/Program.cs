using CloudAppServer.Models;
using System.Text.RegularExpressions;

List<UploadedFile> uploadedFiles = new List<UploadedFile>
{
    new() { Id = Guid.NewGuid().ToString().ToLower(), Title = "FirstTestFile", DateOfUpload = DateTime.Now },
    new() { Id = Guid.NewGuid().ToString().ToLower(), Title = "SecondTestFile", DateOfUpload = DateTime.Now },
    new() { Id = Guid.NewGuid().ToString().ToLower(), Title = "ThirdTestFile", DateOfUpload = DateTime.Now },
};

var builder = WebApplication.CreateBuilder();

var app = builder.Build();

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;
    //string expressionForNumber = "^/api/files/([0 - 9]+)$";   // ���� id ������������ �����

    // 2e752824-1657-4c7f-844b-6ec2e168e99c
    string expressionForGuid = @"^/api/files/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
    if (path == "/api/files" && request.Method == "GET")
    {
        await GetAllUploadedFiles(response);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
    {
        // �������� id �� ������ url
        string? id = path.Value?.Split("/")[3];
        await GetUploadedFile(id, response);
    }
    else if (path == "/api/files" && request.Method == "POST")
    {
        await CreateUploadedFile(response, request);
    }
    else if (path == "/api/files" && request.Method == "PUT")
    {
        await UpdateUploadedFile(response, request);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
    {
        string? id = path.Value?.Split("/")[3];
        await DeleteUploadedFile(id, response);
    }
    else
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("html/index.html");
    }
});

// ��������� ���� �������������
async Task GetAllUploadedFiles(HttpResponse response)
{
    await response.WriteAsJsonAsync(uploadedFiles);
}
// ��������� ������ ������������ �� id
async Task GetUploadedFile(string? id, HttpResponse response)
{
    // �������� ������������ �� id
    UploadedFile? uploadedFile = uploadedFiles.FirstOrDefault((u) => u.Id == id);
    // ���� ������������ ������, ���������� ���
    if (uploadedFile != null)
        await response.WriteAsJsonAsync(uploadedFile);
    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "������������ �� ������" });
    }
}

async Task DeleteUploadedFile(string? id, HttpResponse response)
{
    // �������� ������������ �� id
    UploadedFile? user = uploadedFiles.FirstOrDefault((u) => u.Id == id);
    // ���� ������������ ������, ������� ���
    if (user != null)
    {
        uploadedFiles.Remove(user);
        await response.WriteAsJsonAsync(user);
    }
    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "������������ �� ������" });
    }
}

async Task CreateUploadedFile(HttpResponse response, HttpRequest request)
{
    try
    {
        // �������� ������ ������������
        var uploadedFile = await request.ReadFromJsonAsync<UploadedFile>();
        if (uploadedFile != null)
        {
            // ������������� id ��� ������ ������������
            uploadedFile.Id = Guid.NewGuid().ToString();
            // ��������� ������������ � ������
            uploadedFiles.Add(uploadedFile);
            await response.WriteAsJsonAsync(uploadedFile);
        }
        else
        {
            throw new Exception("������������ ������");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "������������ ������" });
    }
}

async Task UpdateUploadedFile(HttpResponse response, HttpRequest request)
{
    try
    {
        // �������� ������ ������������
        UploadedFile? uploadedFileData = await request.ReadFromJsonAsync<UploadedFile>();
        if (uploadedFileData != null)
        {
            // �������� ������������ �� id
            var user = uploadedFiles.FirstOrDefault(u => u.Id == uploadedFileData.Id);
            // ���� ������������ ������, �������� ��� ������ � ���������� ������� �������
            if (user != null)
            {
                user.Title = uploadedFileData.Title;
                user.DateOfUpload = uploadedFileData.DateOfUpload;
                await response.WriteAsJsonAsync(user);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "������������ �� ������" });
            }
        }
        else
        {
            throw new Exception("������������ ������");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "������������ ������" });
    }
}

app.Run();
