using LinQHomework.Data;
using LinQHomework.Filters;
using LinQHomework.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add database context to dependency injection container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("myDb1")));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


#region Response time header middleware (1):
// https://www.codeproject.com/Tips/5337523/Response-Time-Header-in-ASP-NET-Core
//@desc: In new project, just need to copy the following items: (watch out the namespace!)
//@desc: Middlewares folder -> Interfaces -> IStopwatch + ResponseTimeMiddleware
//@desc: + Filters folder -> ResponseTimeFilter
//@desc: No need to modify any controller
builder.Services.AddScoped<IActionResponseTimeStopwatch, ActionResponseTimeStopwatch>();

/*Filter*/
builder.Services.AddMvc(options =>
{
    options.Filters.Add(new ResponseTimeFilter());
});

builder.Services.AddScoped<IMiddlewareResponseTimeStopwatch, MiddlewareResponseTimeStopwatch>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


/*Middleware (Add Response-Time-Header)*/
app.UseMiddleware<ResponseTimeMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
