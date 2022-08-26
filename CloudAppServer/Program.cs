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
    //string expressionForNumber = "^/api/files/([0 - 9]+)$";   // если id представл€ет число

    // 2e752824-1657-4c7f-844b-6ec2e168e99c
    string expressionForGuid = @"^/api/files/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
    if (path == "/api/files" && request.Method == "GET")
    {
        await GetAllUploadedFiles(response);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
    {
        // получаем id из адреса url
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

// получение всех пользователей
async Task GetAllUploadedFiles(HttpResponse response)
{
    await response.WriteAsJsonAsync(uploadedFiles);
}
// получение одного пользовател€ по id
async Task GetUploadedFile(string? id, HttpResponse response)
{
    // получаем пользовател€ по id
    UploadedFile? uploadedFile = uploadedFiles.FirstOrDefault((u) => u.Id == id);
    // если пользователь найден, отправл€ем его
    if (uploadedFile != null)
        await response.WriteAsJsonAsync(uploadedFile);
    // если не найден, отправл€ем статусный код и сообщение об ошибке
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "ѕользователь не найден" });
    }
}

async Task DeleteUploadedFile(string? id, HttpResponse response)
{
    // получаем пользовател€ по id
    UploadedFile? user = uploadedFiles.FirstOrDefault((u) => u.Id == id);
    // если пользователь найден, удал€ем его
    if (user != null)
    {
        uploadedFiles.Remove(user);
        await response.WriteAsJsonAsync(user);
    }
    // если не найден, отправл€ем статусный код и сообщение об ошибке
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "ѕользователь не найден" });
    }
}

async Task CreateUploadedFile(HttpResponse response, HttpRequest request)
{
    try
    {
        // получаем данные пользовател€
        var uploadedFile = await request.ReadFromJsonAsync<UploadedFile>();
        if (uploadedFile != null)
        {
            // устанавливаем id дл€ нового пользовател€
            uploadedFile.Id = Guid.NewGuid().ToString();
            // добавл€ем пользовател€ в список
            uploadedFiles.Add(uploadedFile);
            await response.WriteAsJsonAsync(uploadedFile);
        }
        else
        {
            throw new Exception("Ќекорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Ќекорректные данные" });
    }
}

async Task UpdateUploadedFile(HttpResponse response, HttpRequest request)
{
    try
    {
        // получаем данные пользовател€
        UploadedFile? uploadedFileData = await request.ReadFromJsonAsync<UploadedFile>();
        if (uploadedFileData != null)
        {
            // получаем пользовател€ по id
            var user = uploadedFiles.FirstOrDefault(u => u.Id == uploadedFileData.Id);
            // если пользователь найден, измен€ем его данные и отправл€ем обратно клиенту
            if (user != null)
            {
                user.Title = uploadedFileData.Title;
                user.DateOfUpload = uploadedFileData.DateOfUpload;
                await response.WriteAsJsonAsync(user);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "ѕользователь не найден" });
            }
        }
        else
        {
            throw new Exception("Ќекорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Ќекорректные данные" });
    }
}

app.Run();
